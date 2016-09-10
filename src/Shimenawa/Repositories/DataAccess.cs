using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.Extensions;
using Medidata.Shimenawa.Models.DB;

namespace Medidata.Shimenawa.Repositories
{
    public class DataAccess : IDataAccess
    {
        public Request CreateRequest(string query, DateTime from, DateTime to, string callbackEndpoint, out bool created)
        {
            created = false;
            using (var context = new SqlDbContext())
            {
                var request = context.Requests.SingleOrDefault(r =>
                    r.Query.Equals(query, StringComparison.InvariantCultureIgnoreCase) &&
                    r.From == from &&
                    r.To == to);

                if (request != null) return request;

                request = new Request
                {
                    RequestUuid = Guid.NewGuid(),
                    Query = query,
                    From = from,
                    To = to,
                    RequestTime = DateTime.UtcNow,
                    StatusMessage = "Request Accepted",
                    CallbackEndpoint = callbackEndpoint
                };
                context.Requests.Add(request);
                context.SaveChanges();
                created = true;

                return request;
            }
        }

        public Request GetRequest(Guid requestUuid)
        {
            using (var context = new SqlDbContext())
            {
                return context.Requests.SingleOrDefault(r =>
                    r.RequestUuid == requestUuid);
            }
        }

        public void UpdateRequest(Guid requestUuid, string status, bool completedSearch = false, bool success = false)
        {
            using (var context = new SqlDbContext())
            {
                var request = context.Requests.SingleOrDefault(r =>
                    r.RequestUuid == requestUuid);
                if (request == null) return;
                
                request.StatusMessage = status;
                if (completedSearch)
                {
                    request.Success = success;
                    request.CompletedRequestTime = DateTime.UtcNow;
                }
                context.SaveChanges();
            }
        }

        public void UpdateRequest(Guid requestUuid, IEnumerable<string> apps, IEnumerable<string> exceptionApps)
        {
            using (var context = new SqlDbContext())
            {
                var request = context.Requests.SingleOrDefault(r =>
                    r.RequestUuid == requestUuid);
                if (request == null) return;

                request.Apps = apps;
                request.ExceptionApps = exceptionApps;
                context.SaveChanges();
            }
        }

        public void DeleteLogs(Guid requestUuid)
        {
            using (var context = new SqlDbContext())
            {
                context.Logs
                    .Where(x => x.RequestUuid == requestUuid)
                    .Delete();
                
                context.SaveChanges();
            }
        }

        public void AddLogs(Guid requestUuid, IEnumerable<Log> logs)
        {
            using (var context = new SqlDbContext())
            {
                context.Logs.AddRange(logs);
                context.SaveChanges();
            }
        }

        public IEnumerable<string> GetLogs(Guid requestUuid)
        {
            using (var context = new SqlDbContext())
            {
                return (from r in context.Requests
                        join l in context.Logs on r.RequestUuid equals l.RequestUuid
                        where r.RequestUuid == requestUuid
                        orderby l.LogId
                        select l.RawLog).ToList();
            }
        }
    }
}