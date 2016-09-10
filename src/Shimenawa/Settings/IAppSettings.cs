using System;

namespace Medidata.Shimenawa.Settings
{
    public interface IAppSettings
    {
        Uri BaseUri { get; }
        Uri SumoApiUri { get; }
        string SumoAccessId { get; }
        string SumoAccessKey { get; }
        int SumoLogWaitTimeBeforeStartSearch { get; }
        int SumoLogIntervalWaitTimeBeforeSearchQueryMs { get; }
        int SumoApiRequestRateLimit { get; }
        int HangfireWorkerCount { get; }
    }
}
