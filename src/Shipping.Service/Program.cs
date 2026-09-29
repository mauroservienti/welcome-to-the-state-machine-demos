using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using System;

namespace Shipping.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceName = typeof(Program).Namespace;
            Console.Title = serviceName;

            var builder = Host.CreateApplicationBuilder(args);
            builder.AddServiceDefaults();

            // Connection strings are injected by .NET Aspire, defaults target the dev container
            var connectionString = builder.Configuration.GetConnectionString("shipping-service-database")
                ?? @"Host=localhost;Port=10432;Username=db_user;Password=P@ssw0rd;Database=shipping_service_database";
            var transportConnectionString = builder.Configuration.GetConnectionString("transport");

            var config = new EndpointConfiguration(serviceName);
            config.ApplyCommonConfigurationWithPersistence(connectionString, tablePrefix: "Shipping", transportConnectionString: transportConnectionString);

            builder.Services.AddNServiceBusEndpoint(config);

            builder.Build().Run();
        }
    }
}
