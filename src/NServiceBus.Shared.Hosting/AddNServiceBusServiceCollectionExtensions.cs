using Microsoft.Extensions.DependencyInjection;
using System;

namespace NServiceBus
{
    public static class AddNServiceBusServiceCollectionExtensions
    {
        public static IServiceProvider AddNServiceBus(this IServiceCollection services, string endpointName, Func<EndpointConfiguration, IServiceProvider> configuration)
        {
            var endpointConfiguration = new EndpointConfiguration(endpointName);

            IMessageSession messageSession = null;
            services.AddSingleton(di => messageSession);

            var container = configuration(endpointConfiguration);

            messageSession = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

            return container;
        }
    }
}
