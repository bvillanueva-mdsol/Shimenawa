using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Medidata.Shimenawa.Helpers;
using Medidata.Shimenawa.Models.DB;
using Medidata.Shimenawa.Models.Sumo;
using Medidata.Shimenawa.Repositories;
using System;
using Medidata.Shimenawa.Settings;

namespace Medidata.Shimenawa.Scheduler.Jobs
{
    public class SearchLogJob : ISearchLogJob
    {
        private const int MaximumSearchLoopTimes = 10;
        private const int MaximumJobAttempts = 3;

        private readonly IAppSettings _appSettings;
        private readonly IDataAccess _dataAccess;
        private readonly IJobManagement _jobManagement;
        private readonly ISumoLogicLogRepository _sumoLogicLogRepository;
        private readonly INotificationJob _notificationJob;

        public SearchLogJob(
            IAppSettings appSettings,
            IDataAccess dataAccess,
            IJobManagement jobManagement,
            ISumoLogicLogRepository sumoLogicLogRepository,
            INotificationJob notificationJob)
        {
            _appSettings = appSettings;
            _dataAccess = dataAccess;
            _jobManagement = jobManagement;
            _sumoLogicLogRepository = sumoLogicLogRepository;
            _notificationJob = notificationJob;
        }

        [AutomaticRetry(Attempts = 0)]
        public void Search(Request request, int attempts)
        {
            try
            {
                SearchLogs(request).Wait();
            }
            catch (Exception)
            {
                var state = $"{attempts} of {MaximumJobAttempts} Search Attempts, Failed. (Please see hangfire dashboard to get detailed exception logs)";
                if (attempts < MaximumJobAttempts)
                {
                    attempts++;
                    _dataAccess.UpdateRequest(request.RequestUuid, state);
                    _jobManagement.Enqueue(() => Search(request, attempts));
                }
                else
                {
                    _dataAccess.UpdateRequest(request.RequestUuid, state, true);
                    NotifyRequester(request.RequestUuid);
                }
                throw;
            }
        }

        public async Task SearchLogs(Request request)
        {
            _dataAccess.UpdateRequest(request.RequestUuid, "Started searching");

            var sumoSurveySearchJobResult = await SurveyLogs(request);

            if (sumoSurveySearchJobResult.SumoSearchJobStatusResponse?.State != SumoSearchJobStatusConstants.DoneGatheringResults)
            {
                var state = sumoSurveySearchJobResult.SumoSearchJobStatusResponse?.State ?? "Undetermined state";
                if (state == SumoSearchJobStatusConstants.ForcePaused) state = "Sumo tagged this search as too big. Search Job stopped.";

                _dataAccess.UpdateRequest(request.RequestUuid, state, true);
                NotifyRequester(request.RequestUuid);
                return;
            }

            _dataAccess.UpdateRequest(request.RequestUuid, "Gathering Logs");

            await FetchAndSaveLogs(request, sumoSurveySearchJobResult.SumoCreateSearchJobResponse, sumoSurveySearchJobResult.SumoSearchJobStatusResponse.MessageCount);
            // delete previous sumoCreateSearchJobResponse search jobid

            _dataAccess.UpdateRequest(request.RequestUuid, "Done Gathering Logs", true, true);
            NotifyRequester(request.RequestUuid);
        }

        private async Task<SumoSurveySearchJobResult> SurveyLogs(Request request)
        {
            SumoCreateSearchJobResponse sumoCreateSearchJobResponse = null;
            var sumoSearchJobStatusResponse = new SumoSearchJobStatusResponse { MessageCount = 0 };
            var previousMessageCountCount = -1;
            var loopTimes = 0;

            while (MaximumSearchLoopTimes > loopTimes &&
                (sumoSearchJobStatusResponse.MessageCount != previousMessageCountCount))
            {
                previousMessageCountCount = sumoSearchJobStatusResponse.MessageCount;
                if (loopTimes > 0) await Task.Delay(_appSettings.SumoLogIntervalWaitTimeBeforeSearchQueryMs);

                // delete previous sumoCreateSearchJobResponse search jobid
                sumoCreateSearchJobResponse =
                    await _sumoLogicLogRepository.CreateSearchJob(request.Query, request.From, request.To);

                sumoSearchJobStatusResponse.State = null;
                while (
                    sumoSearchJobStatusResponse.State == null ||
                    sumoSearchJobStatusResponse.State == SumoSearchJobStatusConstants.NotStarted ||
                    sumoSearchJobStatusResponse.State == SumoSearchJobStatusConstants.GatheringResults)
                {
                    await Task.Delay(1000);
                    sumoSearchJobStatusResponse =
                        await _sumoLogicLogRepository.GetJobStatus(sumoCreateSearchJobResponse);
                }

                if (sumoSearchJobStatusResponse.State == SumoSearchJobStatusConstants.ForcePaused) break;

                loopTimes++;
            }

            return new SumoSurveySearchJobResult
            {
                SumoCreateSearchJobResponse = sumoCreateSearchJobResponse,
                SumoSearchJobStatusResponse =   sumoSearchJobStatusResponse
            };
        }

        private async Task FetchAndSaveLogs(Request request, SumoCreateSearchJobResponse sumoCreateSearchJobResponse, int expectedMessageCount)
        {
            _dataAccess.DeleteLogs(request.RequestUuid);

            var offset = 0;
            var batchLimit = 300;
            var actualMessageCount = 0;
            var apps = new List<string>();
            var exceptionApps = new List<string>();
            while (expectedMessageCount > actualMessageCount)
            {
                var partialMessages = await _sumoLogicLogRepository.GetMessages(
                    request.RequestUuid, sumoCreateSearchJobResponse, offset, batchLimit);
                if (!partialMessages.Any()) break;

                _dataAccess.AddLogs(request.RequestUuid, partialMessages);                
                actualMessageCount += partialMessages.Count;

                foreach (var app in partialMessages.Where(log => !string.IsNullOrWhiteSpace(log.ComponentName)).Select(log => log.ComponentName).Distinct())
                    if (!apps.Contains(app)) apps.Add(app);
                foreach (var exceptionApp in partialMessages.Where(log => log.HasException && !string.IsNullOrWhiteSpace(log.ComponentName)).Select(log => log.ComponentName).Distinct())
                    if (!exceptionApps.Contains(exceptionApp)) exceptionApps.Add(exceptionApp);

                offset += batchLimit;
            }

            _dataAccess.UpdateRequest(request.RequestUuid, apps, exceptionApps);
        }

        private void NotifyRequester(Guid requestUuid)
        {
            var updatedRequest = _dataAccess.GetRequest(requestUuid);
            if (!string.IsNullOrWhiteSpace(updatedRequest.CallbackEndpoint))
            {
                _jobManagement.Enqueue(() => _notificationJob.PublishNotification(requestUuid));
            }
        }
    }
}