using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Medidata.Shimenawa.Helpers;
using Medidata.Shimenawa.Models;
using Medidata.Shimenawa.Repositories;
using Medidata.Shimenawa.Scheduler;
using Newtonsoft.Json;
using Medidata.Shimenawa.Builder;

namespace Medidata.Shimenawa.Controllers.Api.V1
{
    [RoutePrefix("api/v1/requests")]
    public class RequestController : ApiController
    {
        private readonly IDataAccess _dataAccess;
        private readonly ISearchLogJobManagement _searchLogJobManagement;
        private readonly IHalRequestsBuilder _halRequestsBuilder;

        public RequestController(IDataAccess dataAccess, ISearchLogJobManagement searchLogJobManagement, IHalRequestsBuilder halRequestsBuilder)
        {
            _dataAccess = dataAccess;
            _searchLogJobManagement = searchLogJobManagement;
            _halRequestsBuilder = halRequestsBuilder;
        }

        [Route("")]
        [HttpPost]
        public HttpResponseMessage RequestLogSearch([FromBody]LogSearchRequest logSearchRequest)
        {
            var validationMessage = string.Empty;
            if (logSearchRequest == null || !logSearchRequest.Validate(out validationMessage))
            {
                var errorMessageObject = new { message = string.IsNullOrWhiteSpace(validationMessage) ? "Invalid Request Body" : validationMessage };
                var errorContent = new StringContent(JsonConvert.SerializeObject(errorMessageObject), Encoding.UTF8, GeneralConstants.JsonContentType);
                return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = errorContent };
            }

            var fromUtc = logSearchRequest.From.Value.ToUniversalTime();
            var toUtc = logSearchRequest.To.Value.ToUniversalTime();

            bool created;
            var request = _dataAccess.CreateRequest(logSearchRequest.Query, fromUtc, toUtc, logSearchRequest.CallbackEndpoint, out created);
            logSearchRequest.RequestUuid = request.RequestUuid;
            var successContent = new StringContent(JsonConvert.SerializeObject(logSearchRequest), Encoding.UTF8, GeneralConstants.JsonContentType);

            if (created)
            {
                _searchLogJobManagement.EnqueueSearchLog(request);
                return new HttpResponseMessage(HttpStatusCode.Created) { Content = successContent };
            }
            
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = successContent };
        }

        [Route("{uuid:guid}")]
        [HttpGet]
        public HttpResponseMessage GetSearchRequest(Guid uuid)
        {
            var request = _dataAccess.GetRequest(uuid);
            if (request == null)
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            
            var returnContent = new StringContent(_halRequestsBuilder.BuildRequest(request), Encoding.UTF8, GeneralConstants.JsonContentType);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = returnContent };
        }

        [Route("{uuid:guid}/logs")]
        [HttpGet]
        public HttpResponseMessage GetLogs(Guid uuid)
        {
            var request = _dataAccess.GetRequest(uuid);
            if (request == null)
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            var logs = _dataAccess.GetLogs(uuid);
            var json = $@"[{string.Join(",", logs)}]";

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };
        }
    }
}
