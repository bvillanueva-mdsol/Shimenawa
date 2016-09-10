using Medidata.Shimenawa.Models.DB;

namespace Medidata.Shimenawa.Builder
{
    public interface IHalRequestsBuilder
    {
        string BuildRoot();
        string BuildRequest(Request request);
        string BuildLogSearchRequest(Request request);
    }
}