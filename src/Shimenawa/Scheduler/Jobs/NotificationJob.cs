using Hangfire;
using Medidata.Shimenawa.Builder;
using Medidata.Shimenawa.Helpers;
using Medidata.Shimenawa.Repositories;
using Medidata.Shimenawa.Settings;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Medidata.Shimenawa.Scheduler.Jobs
{
    public class NotificationJob : INotificationJob
    {
        private readonly IHalRequestsBuilder _halRequestsBuilder;
        private readonly IDataAccess _dataAccess;
        private readonly IAppSettings _appSettings;

        public NotificationJob(IHalRequestsBuilder halRequestsBuilder, IDataAccess dataAccess, IAppSettings appsettings)
        {
            _halRequestsBuilder = halRequestsBuilder;
            _dataAccess = dataAccess;
            _appSettings = appsettings;
        }

        [AutomaticRetry(Attempts = 10)]
        public void PublishNotification(Guid requestUuid)
        {
            var request = _dataAccess.GetRequest(requestUuid);

            var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                new Uri(request.CallbackEndpoint))
            {
                Content = new StringContent(
                    _halRequestsBuilder.BuildRequest(request),
                    Encoding.UTF8,
                    GeneralConstants.HalJsonContentType)
            };
            
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(GeneralConstants.HalJsonContentType));
                var result = client.SendAsync(httpRequest).Result;
                result.EnsureSuccessStatusCode();
            }
        }
    }
}