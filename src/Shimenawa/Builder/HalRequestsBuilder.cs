using Common.Logging.Configuration;
using Crichton.Representors;
using Crichton.Representors.Serializers;
using Medidata.Shimenawa.Models.DB;
using Medidata.Shimenawa.Settings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Medidata.Shimenawa.Builder
{
    public class HalRequestsBuilder : IHalRequestsBuilder
    {
        private const string RequestPrefix = "api/v1/requests";
        private readonly Uri _baseUri;

        public HalRequestsBuilder(IAppSettings appsetings)
        {
            _baseUri = appsetings.BaseUri;
        }

        public string BuildRequest(Request request)
        {
            var builder = new RepresentorBuilder();

            var representor = CreateRequestRepresentor(request);
            var serializer = new HalSerializer();
            return serializer.Serialize(representor);
        }

        private CrichtonRepresentor CreateRequestRepresentor(Request request)
        {
            var builder = new RepresentorBuilder();

            // attributes
            builder.SetAttributes(new JObject(new List<JProperty>
            {
                new JProperty("request_uuid", request.RequestUuid),
                new JProperty("query", request.Query),
                new JProperty("from", FormatDateTime(request.From)),
                new JProperty("to", FormatDateTime(request.To)),
                new JProperty("success", request.Success),
                new JProperty("status_message", request.StatusMessage),
                new JProperty("request_time", FormatDateTime(request.RequestTime)),
                new JProperty("completed_request_time", FormatDateTime(request.CompletedRequestTime)),
                new JProperty("apps", request.Apps),
                new JProperty("exceptionApps", request.ExceptionApps),
                new JProperty("callback_endpoint", request.CallbackEndpoint ?? string.Empty)
            }));

            // _link
            var issuesUri = new Uri(_baseUri, RequestPrefix);

            builder.SetSelfLink(CreateLink(request.RequestUuid.ToString()));
            builder.AddTransition(new CrichtonTransition
            {
                Rel = "logs",
                Uri = CreateLink(request.RequestUuid.ToString()) + "/logs"
            });

            return builder.ToRepresentor();
        }

        private string CreateLink(string requestUuid)
        {
            var template = new UriTemplate(RequestPrefix + "/{requestUuid}");
            var parameters = new NameValueCollection
            {
                { "requestUuid", requestUuid }
            };

            return template.BindByName(_baseUri, parameters).ToString();
        }

        private static string FormatDateTime(DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return string.Empty;
            }

            return DateTime.SpecifyKind((DateTime)dateTime, DateTimeKind.Utc).ToString("o");
        }
    }
}