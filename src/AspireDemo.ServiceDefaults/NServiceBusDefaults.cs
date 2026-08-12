using Microsoft.Extensions.Configuration;
using NServiceBus;
using NServiceBus.Transport;
using Npgsql;
using NpgsqlTypes;
using NServiceBus.TransactionalSession;

namespace Microsoft.Extensions.Hosting;

public static class NServiceBusDefaults
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddNServiceBusEndpoint(
            string name,
            string persistenceDbName,
            string tablePrefix,
            bool enableOutbox = true,
            bool enableTransactionalSession = false,
            Action<EndpointConfiguration, RoutingSettings>? configureEndpoint = null)
        {
            var endpointConfiguration = new EndpointConfiguration(name);

            #region transport-config
            var transportConnectionString = builder.Configuration.GetConnectionString("transport");
            if (transportConnectionString is null)
            {
                throw new InvalidOperationException
                    ($"No transport configured. Provide a 'ConnectionStrings:transport'.");
            }

            var routing = endpointConfiguration.UseTransport
                (new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), transportConnectionString));
            #endregion

            #region persistence-config
            // 2. Resolve the service's SPECIFIC isolated database string directly from the local process config map [INDEX]
            var persistenceConnectionString = builder.Configuration.GetConnectionString(persistenceDbName);
            if (string.IsNullOrEmpty(persistenceConnectionString))
            {
                throw new InvalidOperationException($"No data persistence configured. Provide a 'ConnectionStrings:{persistenceDbName}'.");
            }

            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            
            persistence.TablePrefix(tablePrefix);
            persistence.ConnectionBuilder(() => new NpgsqlConnection(persistenceConnectionString));

            
            var dialect = persistence.SqlDialect<SqlDialect.PostgreSql>();

            // Centralized JsonB Parameter Modifier to protect Npgsql drivers from serializing crashes [INDEX]
            dialect.JsonBParameterModifier(modifier: parameter =>
            {
                var npgsqlParameter = (NpgsqlParameter)parameter;
                npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
            });

            
            if (enableOutbox)
            {
                endpointConfiguration.EnableOutbox();
            }

            if (enableTransactionalSession)
            {
                persistence.EnableTransactionalSession();
            }
            #endregion






            endpointConfiguration.UseSerialization<SystemJsonSerializer>();
            endpointConfiguration.SendHeartbeatTo("Particular.ServiceControl");
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.AuditSagaStateChanges(serviceControlQueue: "Particular.ServiceControl");


            var metrics = endpointConfiguration.EnableMetrics();
            metrics.SendMetricDataToServiceControl("Particular.Monitoring", TimeSpan.FromSeconds(1));

            var messageConventions = endpointConfiguration.Conventions();
            messageConventions.DefiningMessagesAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages"));
            messageConventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages.Events"));
            messageConventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages.Commands"));


            #region enable-installers
            endpointConfiguration.EnableInstallers();
            #endregion

            configureEndpoint?.Invoke(endpointConfiguration, routing);

            builder.Services.AddNServiceBusEndpoint(endpointConfiguration);
            
            return builder;
        }
    }
}