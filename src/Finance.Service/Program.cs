using Finance.PaymentGateway.Messages;
using NServiceBus;
using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Finance.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceName = typeof(Program).Namespace;
            Console.Title = serviceName;

            CreateHostBuilder(serviceName, args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string serviceName, string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureLogging((ctx, logging) =>
                {
                    logging.AddConfiguration(ctx.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                })
                .UseNServiceBus(ctx =>
                {
                    const string connectionString = @"Host=localhost;Port=7432;Username=db_user;Password=P@ssw0rd;Database=finance_service_database";
                    var config = new EndpointConfiguration(serviceName);
                    config.ApplyCommonConfigurationWithPersistence(connectionString, tablePrefix:"Finance", configureRouting: routing =>
                    {
                        routing.RouteToEndpoint(typeof(AuthorizeCard), "Finance.PaymentGateway");
                        routing.RouteToEndpoint(typeof(ReleaseCardAuthorization), "Finance.PaymentGateway");
                        routing.RouteToEndpoint(typeof(ChargeCard), "Finance.PaymentGateway");
                    });

                    return config;
                });

            return builder;
        }
    }
}
