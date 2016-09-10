using System;
using System.Linq.Expressions;
using Hangfire;

namespace Medidata.Shimenawa.Scheduler
{
    public class HangfireJobManagement : IJobManagement
    {
        public void Schedule(Expression<Action> methodCall, double timeDelayMs)
        {
            BackgroundJob.Schedule(methodCall, TimeSpan.FromMilliseconds(timeDelayMs));
        }

        public void Enqueue(Expression<Action> methodCall)
        {
            BackgroundJob.Enqueue(methodCall);
        }
    }
}