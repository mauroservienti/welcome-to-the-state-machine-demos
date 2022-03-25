using Microsoft.Data.SqlClient;
using System;

namespace NServiceBus
{
    public static class CommonEndpointSettings
    {
        public static void ApplyCommonConfiguration(this EndpointConfiguration endpointConfiguration, Action<RoutingSettings<LearningTransport>> configureRouting = null)
        {
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            var transportConfig = endpointConfiguration.UseTransport<LearningTransport>();
            configureRouting?.Invoke(transportConfig.Routing());

            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.SendFailedMessagesTo("error");

            endpointConfiguration.SendHeartbeatTo(
                serviceControlQueue: "Particular.ServiceControl",
                frequency: TimeSpan.FromSeconds(10),
                timeToLive: TimeSpan.FromSeconds(5));

            endpointConfiguration.AuditSagaStateChanges(
                serviceControlQueue: "Particular.ServiceControl");

            var messageConventions = endpointConfiguration.Conventions();
            messageConventions.DefiningMessagesAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages"));
            messageConventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages.Events"));
            messageConventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages.Commands"));

            var metrics = endpointConfiguration.EnableMetrics();
            metrics.SendMetricDataToServiceControl(
                serviceControlMetricsAddress: "Particular.Monitoring",
                interval: TimeSpan.FromSeconds(5));
        }

        public static void ApplyCommonConfigurationWithPersistence(this EndpointConfiguration endpointConfiguration, string sqlPersistenceConnectionString, Action<RoutingSettings<LearningTransport>> configureRouting = null)
        {
            ApplyCommonConfiguration(endpointConfiguration, configureRouting);

            endpointConfiguration.EnableInstallers();

            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            persistence.SqlDialect<SqlDialect.MsSqlServer>();
            persistence.ConnectionBuilder(() => new SqlConnection(sqlPersistenceConnectionString));

            endpointConfiguration.EnableOutbox();
        }
    }
}
