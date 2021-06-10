using Hangfire.Annotations;
using LightInject;
using System;

namespace Hangfire
{
    public static class GlobalConfigurationExtensions
    {
        public static IGlobalConfiguration<LightInjectJobActivator> UseLightInjectActivator(
           [NotNull] this IGlobalConfiguration configuration,
           [NotNull] ServiceContainer container)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (container == null) throw new ArgumentNullException("container");

            return configuration.UseActivator(new LightInjectJobActivator(container));
        }
    }
}
