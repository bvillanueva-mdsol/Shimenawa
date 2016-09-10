using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Medidata.Shimenawa.Helpers;
using Medidata.Shimenawa.Models.DB;
using Medidata.Shimenawa.Models.Sumo;
using Medidata.Shimenawa.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Medidata.Shimenawa.Repositories
{
    public class SumoLogicLogRepository : ISumoLogicLogRepository, IDisposable
    {
        private const string CreateSearchJobPath = "search/jobs";
        private const string ErrorKeyword = "exception";
        private const string ScreenshotKey = "mcc_screenshot";
        private const string ScreenRepresentationStr = "SCREEN-REPRESENTATION";

        private readonly IAppSettings _appSettings;
        private readonly SemaphoreSlim _pool;

        public SumoLogicLogRepository(IAppSettings appSettings)
        {
            _appSettings = appSettings;
            _pool = new SemaphoreSlim(appSettings.SumoApiRequestRateLimit, appSettings.SumoApiRequestRateLimit);
        }

        public async Task<SumoCreateSearchJobResponse> CreateSearchJob(string query, DateTime from, DateTime to)
        {
            var resource = new
            {
                query,
                from = $"{from.ToString("s")}Z",
                to = $"{to.ToString("s")}Z"
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                new Uri(_appSettings.SumoApiUri, CreateSearchJobPath))
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(resource),
                    Encoding.UTF8,
                    GeneralConstants.JsonContentType)
            };

            var response = await SendAsync(request);
            response.EnsureSuccessStatusCode();

            return new SumoCreateSearchJobResponse()
            {
                Location = response.Headers?.Location,
                CookieContainer = ReadCookies(response)
            };
        }

        public async Task<SumoSearchJobStatusResponse> GetJobStatus(SumoCreateSearchJobResponse createSearchJobResponse)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, createSearchJobResponse.Location);
            var response = await SendAsync(request, createSearchJobResponse.CookieContainer);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SumoSearchJobStatusResponse>(body);
        }

        public async Task<List<Log>> GetMessages(Guid requestUuid, SumoCreateSearchJobResponse createSearchJobResponse, int offset, int limit)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"{createSearchJobResponse.Location}/messages?offset={offset}&limit={limit}"));
            var response = await SendAsync(request, createSearchJobResponse.CookieContainer);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var rawSumoMessages = JsonConvert.DeserializeObject<SumoLogMessageResponse>(body);
            return ExtractLogs(requestUuid, rawSumoMessages);
        }

        private List<Log> ExtractLogs(Guid requestUuid, SumoLogMessageResponse sumoLogMessageResponse)
        {
            var logs = new List<Log>();
            var sumoMessages = sumoLogMessageResponse?.Messages;
            if (sumoMessages == null) return logs;
            var sumoLogMapMessages = sumoMessages.Select(x => x.Map).ToList();

            foreach (var sumoMessage in sumoLogMapMessages)
            {
                var jsonLog = sumoMessage.RawMessage;
                var model = new Log { RequestUuid = requestUuid };

                try
                {
                    var dataLog = JObject.Parse(jsonLog);
                    var message = dataLog["message"].ToString();
                    model.ComponentName = dataLog["component"].ToString().ToLower();

                    var exception = string.Empty;
                    if (dataLog["exception"] != null)
                    {
                        exception = dataLog["exception"].ToString();
                    }
                    model.HasException = message.IndexOf(ErrorKeyword, StringComparison.CurrentCultureIgnoreCase) != -1
                           || !string.IsNullOrWhiteSpace(exception);

                    model.RawLog = ReplaceBinaryToScreenRepresentationStr(jsonLog);
                }
                catch (Exception)
                {
                    model.HasException = false;
                    model.ComponentName = null;
                    model.RawLog = JObject.FromObject(new { message = jsonLog }).ToString(Formatting.None);
                }

                logs.Add(model);
            }

            return logs;
        }

        private string ReplaceBinaryToScreenRepresentationStr(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            if (!raw.StartsWith("{"))
            {
                var pieceToRemove = raw.Substring(0, raw.IndexOf(','));
                raw = raw.Replace(pieceToRemove, ScreenRepresentationStr);
            }

            if (raw.IndexOf(ScreenshotKey) > -1)
            {
                var startOfScreenShot = raw.IndexOf(ScreenshotKey);
                var pieceToRemove = raw.Substring(startOfScreenShot, raw.Length - startOfScreenShot);
                raw = raw.Replace(pieceToRemove, ScreenshotKey + ScreenRepresentationStr);
            }

            return raw;
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CookieContainer cookieContainer = null)
        {
            // TODO: apply retry with random sleep time when hitting error 429 instead of doing semaphore style
            // current implementation will prohibit scaling of servers
            _pool.Wait();
            try
            {
                var handler = new HttpClientHandler();
                if (cookieContainer != null) handler.CookieContainer = cookieContainer;

                using (var client = new HttpClient(handler))
                {
                    var authorization = $"{_appSettings.SumoAccessId}:{_appSettings.SumoAccessKey}";
                    var byteArray = Encoding.ASCII.GetBytes(authorization);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(byteArray));
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue(GeneralConstants.JsonContentType));

                    return await client.SendAsync(request).ConfigureAwait(false);
                }
            }
            finally
            {
                _pool.Release();
            }
        }

        private static CookieContainer ReadCookies(HttpResponseMessage response)
        {
            var pageUri = response.RequestMessage.RequestUri;

            var cookieContainer = new CookieContainer();
            IEnumerable<string> cookies;
            if (!response.Headers.TryGetValues("set-cookie", out cookies)) return cookieContainer;
            foreach (var c in cookies)
            {
                cookieContainer.SetCookies(pageUri, c);
            }

            return cookieContainer;
        }

        public void Dispose()
        {
            _pool.Dispose();
        }
    }
}