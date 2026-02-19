using System;
using Npgsql;
using NpgsqlTypes;
using NServiceBus.TransactionalSession;

namespace NServiceBus
{
    public static class CommonEndpointSettings
    {
        public static void ApplyCommonConfiguration(this EndpointConfiguration endpointConfiguration, Action<RoutingSettings<RabbitMQTransport>> configureRouting = null)
        {
            endpointConfiguration.EnableInstallers();
            
            endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
            
            var routeSettings = endpointConfiguration.UseTransport(new RabbitMQTransport(
                    RoutingTopology.Conventional(QueueType.Classic),
                    "host=localhost"
                )
            );
            configureRouting?.Invoke(routeSettings);

            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.SendFailedMessagesTo("error");

            var messageConventions = endpointConfiguration.Conventions();
            messageConventions.DefiningMessagesAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages"));
            messageConventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages.Events"));
            messageConventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages.Commands"));
        }

        public static void ApplyCommonConfigurationWithPersistence(this EndpointConfiguration endpointConfiguration, string sqlPersistenceConnectionString, string tablePrefix = null, Action<RoutingSettings<RabbitMQTransport>> configureRouting = null)
        {
            ApplyCommonConfiguration(endpointConfiguration, configureRouting);

            ConfigureSqlPersistence(endpointConfiguration, sqlPersistenceConnectionString, tablePrefix);

            endpointConfiguration.EnableOutbox();
        }

        public static void ApplyWebsiteConfigurationWithPersistence(this EndpointConfiguration endpointConfiguration, string sqlPersistenceConnectionString)
        {
            ApplyCommonConfiguration(endpointConfiguration);

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
