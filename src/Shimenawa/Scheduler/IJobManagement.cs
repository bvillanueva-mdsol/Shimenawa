using System;
using System.Linq.Expressions;

namespace Medidata.Shimenawa.Scheduler
{
    public interface IJobManagement
    {
        void Schedule(Expression<Action> methodCall, double timeDelayMs);
        void Enqueue(Expression<Action> methodCall);
    }
}