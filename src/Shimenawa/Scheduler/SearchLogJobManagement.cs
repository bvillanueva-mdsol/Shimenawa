using System;
using Medidata.Shimenawa.Models.DB;
using Medidata.Shimenawa.Scheduler.Jobs;
using Medidata.Shimenawa.Settings;

namespace Medidata.Shimenawa.Scheduler
{
    public class SearchLogJobManagement : ISearchLogJobManagement
    {
        private readonly IAppSettings _appSettings;
        private readonly ISearchLogJob _searchLogJob;
        private readonly IJobManagement _jobManagement;

        public SearchLogJobManagement(IAppSettings appSettings, ISearchLogJob searchLogJob, IJobManagement jobManagement)
        {
            _appSettings = appSettings;
            _searchLogJob = searchLogJob;
            _jobManagement = jobManagement;
        }

        public void EnqueueSearchLog(Request request)
        {
            _jobManagement.Schedule(() => _searchLogJob.Search(request, 1), CalculateWaitAllowanceTime(request.To, request.RequestTime));
        }

        /// <summary>
        /// Provides clearance time before starting search. There will be times that sumo clients are late to submit logs to sumo server.
        /// TODO: This can be provided by requester
        /// </summary>
        /// <param name="to"></param>
        /// <param name="requestAccepted"></param>
        /// <returns></returns>
        private int CalculateWaitAllowanceTime(DateTime to, DateTime requestAccepted)
        {
            double timeAllowanceMs = _appSettings.SumoLogWaitTimeBeforeStartSearch;
            var timeDifferenceMs = (requestAccepted - to).TotalMilliseconds;

            var timeDelayMs = timeAllowanceMs;
            if (timeDifferenceMs >= timeAllowanceMs) { timeDelayMs = 0; }
            else if (timeDifferenceMs > 0) { timeDelayMs = timeAllowanceMs - timeDifferenceMs; }

            return (int)timeDelayMs;
        }
    }
}