using Medidata.Shimenawa.Models.DB;

namespace Medidata.Shimenawa.Scheduler
{
    public interface ISearchLogJobManagement
    {
        void EnqueueSearchLog(Request request);
    }
}