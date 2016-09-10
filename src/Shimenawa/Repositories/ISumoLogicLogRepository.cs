using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Medidata.Shimenawa.Models.DB;
using Medidata.Shimenawa.Models.Sumo;

namespace Medidata.Shimenawa.Repositories
{
    public interface ISumoLogicLogRepository
    {
        Task<SumoCreateSearchJobResponse> CreateSearchJob(string query, DateTime from, DateTime to);
        Task<SumoSearchJobStatusResponse> GetJobStatus(SumoCreateSearchJobResponse createSearchJobResponse);
        Task<List<Log>> GetMessages(Guid requestUuid, SumoCreateSearchJobResponse createSearchJobResponse, int offset, int limit);
    }
}