using Microsoft.Extensions.DependencyInjection;
using System;

namespace NServiceBus
{
    public static class CommonEndpointSettings
    {
        public static (IServiceCollection Services, EndpointConfiguration EndpointConfiguration) ApplyCommonConfiguration(this (IServiceCollection Services, EndpointConfiguration EndpointConfiguration) userConfig, bool asSendOnly = false, Action<RoutingSettings<LearningTransport>> configureRouting = null)
        {
            var endpointConfiguration = userConfig.EndpointConfiguration;

            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            var transportConfig = endpointConfiguration.UseTransport<LearningTransport>();
            configureRouting?.Invoke(transportConfig.Routing());

            if (!asSendOnly)
            {
                endpointConfiguration.UsePersistence<LearningPersistence>();
            }

            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.SendFailedMessagesTo("error");

            endpointConfiguration.SendHeartbeatTo(
                serviceControlQueue: "Particular.ServiceControl",
                frequency: TimeSpan.FromSeconds(10),
                timeToLive: TimeSpan.FromSeconds(5));

            var messageConventions = endpointConfiguration.Conventions();
            messageConventions.DefiningMessagesAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages"));
            messageConventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages.Events"));
            messageConventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages.Commands"));

            if (asSendOnly)
            {
                endpointConfiguration.SendOnly();
            }
            else
            {
                var metrics = endpointConfiguration.EnableMetrics();
                metrics.SendMetricDataToServiceControl(
                    serviceControlMetricsAddress: "Particular.Monitoring",
                    interval: TimeSpan.FromSeconds(5));
            }

            return userConfig;
        }
    }
}
