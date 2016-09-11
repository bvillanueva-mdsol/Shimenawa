using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Medidata.Shimenawa.Helpers;
using Medidata.Shimenawa.Builder;

namespace Medidata.Shimenawa.Controllers.Api.V1
{
    [RoutePrefix("api/v1")]
    public class RootController : ApiController
    {
        private readonly IHalRequestsBuilder _halRequestsBuilder;

        public RootController(IHalRequestsBuilder halRequestsBuilder)
        {
            _halRequestsBuilder = halRequestsBuilder;
        }

        [Route("")]
        [HttpGet]
        public HttpResponseMessage RequestRoot()
        {
            var returnContent = new StringContent(_halRequestsBuilder.BuildRoot(), Encoding.UTF8, GeneralConstants.HalJsonContentType);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = returnContent };
        }
    }
}
