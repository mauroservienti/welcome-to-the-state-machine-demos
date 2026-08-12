using Microsoft.Extensions.Configuration;
using System;

namespace ITOps.Infrastructure
{
    public static class ConnectionStringProvider
    {
        private static IConfiguration? _configuration;

        // 🌟 Initialized exactly once on application startup
        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static string GetConnectionString(string databaseName)
        {
            if (_configuration == null)
            {
                throw new InvalidOperationException(
                    "ITOps Critical Error: ConnectionStringProvider has not been initialized. " +
                    "Make sure Initialize() is called during the application startup pipeline.");
            }

            var connectionString = _configuration.GetConnectionString(databaseName);

            // 🪓 FAIL FAST: Centralized infrastructure validation!
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"ITOps Critical Error: The connection string for database '{databaseName}' could not be resolved. " +
                    "Verify that your .NET Aspire AppHost includes a valid '.WithReference()' link to this process.");
            }

            return connectionString;
        }
    }
}
