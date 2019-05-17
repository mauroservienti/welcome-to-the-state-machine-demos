using Microsoft.Extensions.DependencyInjection;

namespace NServiceBus
{
    public static class AddNServiceBusServiceCollectionExtensions
    {
        static void AddRequiredInfrastructure(IServiceCollection services, EndpointConfiguration configuration)
        {
            var holder = new SessionAndConfigurationHolder(configuration);
            services.AddSingleton(provider => holder.Session);
            services.AddSingleton(holder);
            services.AddHostedService<EndpointManagement>();
        }

        public static (IServiceCollection Services, EndpointConfiguration EndpointConfiguration) AddNServiceBus(this IServiceCollection services, string endpointName)
        {
            var endpointConfiguration = new EndpointConfiguration(endpointName);
            AddRequiredInfrastructure(services, endpointConfiguration);

            return (services, endpointConfiguration);
        }
    }
}
