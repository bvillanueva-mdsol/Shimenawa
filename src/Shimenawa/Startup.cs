using System;
using Hangfire;
using Medidata.Shimenawa;
using Medidata.Shimenawa.Repositories;
using Medidata.Shimenawa.Settings;
using Microsoft.Owin;
using Microsoft.Practices.Unity;
using Owin;
using Hangfire.Dashboard;
using System.Linq;

[assembly: OwinStartup(typeof(Startup))]

namespace Medidata.Shimenawa
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var container = UnityConfig.GetConfiguredContainer();
            var appSettings = container.Resolve<IAppSettings>();

            // contruct EF DB for first time access
            container.Resolve<IDataAccess>().GetRequest(Guid.NewGuid());
            GlobalConfiguration.Configuration.UseUnityActivator(container);
            GlobalConfiguration.Configuration.UseSqlServerStorage("Shimenawa");

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = Enumerable.Empty<IDashboardAuthorizationFilter>()
            });
            app.UseHangfireServer(new BackgroundJobServerOptions { WorkerCount = appSettings.HangfireWorkerCount });
        }
    }
}
