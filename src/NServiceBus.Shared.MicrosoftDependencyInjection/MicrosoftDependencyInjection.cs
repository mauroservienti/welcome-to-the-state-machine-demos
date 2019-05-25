using Microsoft.Extensions.DependencyInjection;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using System;

namespace NServiceBus
{
    public static class MicrosoftDependencyInjection
    {
        public static IServiceProvider UseMicrosoftDependencyInjection(this EndpointConfiguration endpointConfiguration, IServiceCollection services)
        {
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
