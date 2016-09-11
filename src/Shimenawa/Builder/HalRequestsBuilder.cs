using Common.Logging.Configuration;
using Crichton.Representors;
using Crichton.Representors.Serializers;
using Medidata.Shimenawa.Helpers;
using Medidata.Shimenawa.Models.DB;
using Medidata.Shimenawa.Settings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Medidata.Shimenawa.Builder
{
    public class HalRequestsBuilder : IHalRequestsBuilder
    {
        private const string RootPrefix = "api/v1";
        private const string RequestPrefix = "api/v1/requests";
        private readonly Uri _baseUri;

        public HalRequestsBuilder(IAppSettings appsetings)
        {
            _baseUri = appsetings.BaseUri;
        }

        public string BuildRoot()
        {
            var builder = new RepresentorBuilder();
            
            // _link
            var rootsUri = new Uri(_baseUri, RootPrefix);
            var requestUri = new Uri(_baseUri, RequestPrefix);
            builder.SetSelfLink(rootsUri.ToString());
            builder.AddTransition(new CrichtonTransition
            {
                Rel = "create_request",
                Title = "Create Sumo Log Search Request",
                Uri = requestUri.OriginalString,
                Type = GeneralConstants.JsonContentType
            });
            builder.AddTransition(new CrichtonTransition
            {
                Rel = "request_id",
                Uri = requestUri + "{?request_uuid}",
                UriIsTemplated = true,
            });

            var representor = builder.ToRepresentor();
            var serializer = new HalSerializer();
            return serializer.Serialize(representor);
        }

        public string BuildRequest(Request request)
        {
            var representor = CreateRequestRepresentor(request);
            var serializer = new HalSerializer();
            return serializer.Serialize(representor);
        }

        public string BuildLogSearchRequest(Request request)
        {
            var representor = CreateLogSearchRequestRepresentor(request);
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
            var requestUri = new Uri(_baseUri, RequestPrefix);

            builder.SetSelfLink(CreateLink(request.RequestUuid.ToString()));
            builder.AddTransition(new CrichtonTransition
            {
                Rel = "logs",
                Uri = CreateLink(request.RequestUuid.ToString()) + "/logs"
            });

            return builder.ToRepresentor();
        }

        private CrichtonRepresentor CreateLogSearchRequestRepresentor(Request request)
        {
            var builder = new RepresentorBuilder();

            // attributes
            builder.SetAttributes(new JObject(new List<JProperty>
            {
                new JProperty("request_uuid", request.RequestUuid),
                new JProperty("query", request.Query),
                new JProperty("from", FormatDateTime(request.From)),
                new JProperty("to", FormatDateTime(request.To)),
                new JProperty("callback_endpoint", request.CallbackEndpoint ?? string.Empty)
            }));

            // _link
            var requestUri = new Uri(_baseUri, RequestPrefix);

            builder.SetSelfLink(CreateLink(request.RequestUuid.ToString()));

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