using Microsoft.Extensions.DependencyInjection;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using System;

namespace NServiceBus
{
    public static class MicrosoftDependencyInjection
    {
        public static IServiceProvider UseMicrosoftDependencyInjection(this (IServiceCollection Services, EndpointConfiguration EndpointConfiguration) userConfig)
        {
            var endpointConfiguration = userConfig.EndpointConfiguration;
            var services = userConfig.Services;

            UpdateableServiceProvider container = null;
            endpointConfiguration.UseContainer<ServicesBuilder>(c =>
            {
                c.ExistingServices(services);
                c.ServiceProviderFactory(sc =>
                {
                    container = new UpdateableServiceProvider(services);
                    return container;
                });
            });

            return container;
        }
    }
}
