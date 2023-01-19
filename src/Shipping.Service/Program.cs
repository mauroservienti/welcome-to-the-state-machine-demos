using NServiceBus;
using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shipping.Service
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
                    const string connectionString = @"Host=localhost;Port=10432;Username=db_user;Password=P@ssw0rd;Database=shipping_service_database";
                    var config = new EndpointConfiguration(serviceName);
                    config.ApplyCommonConfigurationWithPersistence(connectionString);

                    return config;
                });

            return builder;
        }
    }
}