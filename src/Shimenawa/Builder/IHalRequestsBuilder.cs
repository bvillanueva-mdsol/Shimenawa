using Medidata.Shimenawa.Models.DB;

namespace Medidata.Shimenawa.Builder
{
    public interface IHalRequestsBuilder
    {
        string BuildRequest(Request request);
    }
}