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
                    var config = new EndpointConfiguration(serviceName);
                    config.ApplyCommonConfigurationWithPersistence(@"Data Source=(localdb)\welcome-to-the-state-machine;Initial Catalog=Shipping;Integrated Security=True");

                    return config;
                });

            return builder;
        }
    }
}