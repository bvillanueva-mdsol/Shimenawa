using Medidata.Shimenawa.Models.DB;

namespace Medidata.Shimenawa.Scheduler.Jobs
{
    public interface ISearchLogJob
    {
        void Search(Request request, int attempts);
    }
}