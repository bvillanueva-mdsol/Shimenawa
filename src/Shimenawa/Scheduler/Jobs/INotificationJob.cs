using System;

namespace Medidata.Shimenawa.Scheduler.Jobs
{
    public interface INotificationJob
    {
        void PublishNotification(Guid requestUuid);
    }
}