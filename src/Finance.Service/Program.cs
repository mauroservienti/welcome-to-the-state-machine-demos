using Finance.PaymentGateway.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using System;

namespace Finance.Service
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
            var connectionString = builder.Configuration.GetConnectionString("finance-service-database")
                ?? @"Host=localhost;Port=7432;Username=db_user;Password=P@ssw0rd;Database=finance_service_database";

            var config = new EndpointConfiguration(serviceName);
            config.ApplyCommonConfigurationWithPersistence(connectionString, tablePrefix: "Finance", configuration: builder.Configuration, configureRouting: routing =>
            {
                routing.RouteToEndpoint(typeof(AuthorizeCard), "Finance.PaymentGateway");
                routing.RouteToEndpoint(typeof(ReleaseCardAuthorization), "Finance.PaymentGateway");
                routing.RouteToEndpoint(typeof(ChargeCard), "Finance.PaymentGateway");
            });

            builder.Services.AddNServiceBusEndpoint(config);

            builder.Build().Run();
        }
    }
}
