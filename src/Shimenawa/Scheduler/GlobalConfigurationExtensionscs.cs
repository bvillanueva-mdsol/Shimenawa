using System;
using Hangfire;
using Hangfire.Annotations;
using Microsoft.Practices.Unity;

namespace Medidata.Shimenawa.Scheduler
{
    public static class GlobalConfigurationExtensions
    {
        public static IGlobalConfiguration<UnityJobActivator> UseUnityActivator(
            [NotNull] this IGlobalConfiguration configuration, IUnityContainer container)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (container == null) throw new ArgumentNullException(nameof(container));

            return configuration.UseActivator(new UnityJobActivator(container));
        }
    }
}