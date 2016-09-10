using System.Data.Entity;
using Medidata.Shimenawa.Models.DB;

namespace Medidata.Shimenawa.Repositories
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext() : base("Shimenawa")
        {
        }

        public DbSet<Request> Requests { get; set; }

        public DbSet<Log> Logs { get; set; }
    }
}