using System;
using Medidata.Shimenawa.Repositories;
using Medidata.Shimenawa.Scheduler;
using Medidata.Shimenawa.Scheduler.Jobs;
using Medidata.Shimenawa.Settings;
using Microsoft.Practices.Unity;
using Medidata.Shimenawa.Builder;

namespace Medidata.Shimenawa
{
    public static class UnityConfig
    {
        private static readonly Lazy<IUnityContainer> Container = new Lazy<IUnityContainer>(() =>
        {
            var unityContainer = new UnityContainer();
            RegisterComponents(unityContainer);

            return unityContainer;

        });

        public static IUnityContainer GetConfiguredContainer()
        {
            return Container.Value;
        }

        private static void RegisterComponents(IUnityContainer container)
        {
            container
                .RegisterType<IAppSettings, AppSettings>(new ContainerControlledLifetimeManager())
                .RegisterType<IDataAccess, DataAccess>(new TransientLifetimeManager())
                .RegisterType<INotificationJob, NotificationJob>(new TransientLifetimeManager())
                .RegisterType<ISumoLogicLogRepository, SumoLogicLogRepository>(new ContainerControlledLifetimeManager())
                .RegisterType<IJobManagement, HangfireJobManagement>(new TransientLifetimeManager())
                .RegisterType<ISearchLogJob, SearchLogJob>(new TransientLifetimeManager())
                .RegisterType<ISearchLogJobManagement, SearchLogJobManagement>(new TransientLifetimeManager())
                .RegisterType<IHalRequestsBuilder, HalRequestsBuilder>(new ContainerControlledLifetimeManager());
        }
    }
}