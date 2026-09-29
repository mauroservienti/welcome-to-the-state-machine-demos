using System;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using NServiceBus.TransactionalSession;

namespace NServiceBus
{
    public static class CommonEndpointSettings
    {
        const string DefaultTransportConnectionString = "host=localhost";

        public static void ApplyCommonConfiguration(this EndpointConfiguration endpointConfiguration, Action<RoutingSettings<RabbitMQTransport>> configureRouting = null, IConfiguration configuration = null)
        {
            endpointConfiguration.EnableInstallers();
            
            endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
            
            // When running via .NET Aspire, the transport and management API
            // settings are provided via configuration; defaults target the dev container
            var transport = new RabbitMQTransport(
                RoutingTopology.Conventional(QueueType.Classic),
                configuration?.GetConnectionString("transport") ?? DefaultTransportConnectionString
            );

            var managementApiUrl = configuration?["RabbitMQ:ManagementApi:Url"];
            if (!string.IsNullOrWhiteSpace(managementApiUrl))
            {
                transport.ManagementApiConfiguration = new ManagementApiConfiguration(
                    managementApiUrl,
                    configuration["RabbitMQ:ManagementApi:UserName"],
                    configuration["RabbitMQ:ManagementApi:Password"]);
            }

            var routeSettings = endpointConfiguration.UseTransport(transport);
            configureRouting?.Invoke(routeSettings);

            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.SendFailedMessagesTo("error");

            var messageConventions = endpointConfiguration.Conventions();
            messageConventions.DefiningMessagesAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages"));
            messageConventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages.Events"));
            messageConventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages.Commands"));
        }

        public static void ApplyCommonConfigurationWithPersistence(this EndpointConfiguration endpointConfiguration, string sqlPersistenceConnectionString, string tablePrefix = null, Action<RoutingSettings<RabbitMQTransport>> configureRouting = null, IConfiguration configuration = null)
        {
            ApplyCommonConfiguration(endpointConfiguration, configureRouting, configuration);

            ConfigureSqlPersistence(endpointConfiguration, sqlPersistenceConnectionString, tablePrefix);

            endpointConfiguration.EnableOutbox();
        }

        public static void ApplyWebsiteConfigurationWithPersistence(this EndpointConfiguration endpointConfiguration, string sqlPersistenceConnectionString, IConfiguration configuration = null)
        {
            ApplyCommonConfiguration(endpointConfiguration, configuration: configuration);

            ConfigureSqlPersistence(endpointConfiguration, sqlPersistenceConnectionString);
        }

        private static void ConfigureSqlPersistence(EndpointConfiguration endpointConfiguration, string sqlPersistenceConnectionString, string tablePrefix = null)
        {
            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            var dialect = persistence.SqlDialect<SqlDialect.PostgreSql>();
            if (!string.IsNullOrWhiteSpace(tablePrefix))
            {
                persistence.TablePrefix(tablePrefix);
            }

            dialect.JsonBParameterModifier(
                modifier: parameter =>
                {
                    var npgsqlParameter = (NpgsqlParameter)parameter;
                    npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
                });
            persistence.ConnectionBuilder(
                connectionBuilder: () => new NpgsqlConnection(sqlPersistenceConnectionString));

            persistence.EnableTransactionalSession();
        }
    }
}
