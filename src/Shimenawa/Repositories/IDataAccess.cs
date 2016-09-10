using System;
using System.Collections.Generic;
using Medidata.Shimenawa.Models.DB;

namespace Medidata.Shimenawa.Repositories
{
    public interface IDataAccess
    {
        Request CreateRequest(string query, DateTime from, DateTime to, string callbackEndpoint, out bool created);
        Request GetRequest(Guid requestUuid);
        void UpdateRequest(Guid requestUuid, IEnumerable<string> apps, IEnumerable<string> exceptionApps);
        void UpdateRequest(Guid requestUuid, string status, bool completedSearch = false, bool success = false);
        void DeleteLogs(Guid requestUuid);
        void AddLogs(Guid requestUuid, IEnumerable<Log> logs);
        IEnumerable<string> GetLogs(Guid requestUuid);
    }
}
