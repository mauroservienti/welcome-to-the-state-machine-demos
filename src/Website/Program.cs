using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

using NServiceBus;
using NServiceBus.TransactionalSession;
using ITOps.Infrastructure;


namespace Website
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ITOps maps the configuration provider exactly ONCE on boot!
            ConnectionStringProvider.Initialize(builder.Configuration);

            builder.AddServiceDefaults();

            builder.AddNServiceBusEndpoint(
                name: "Webapp",
                persistenceDbName: "website-db",
                tablePrefix: "",
                enableOutbox: true,
                enableTransactionalSession: true
            );

            var startup = new Startup();
            startup.ConfigureServices(builder.Services);

            var app = builder.Build();
            startup.Configure(app);

            // 4. Map Aspire default health/alive endpoints to make the dashboards green
            app.MapDefaultEndpoints();


            app.Run();
        }
    }
}




// using Microsoft.AspNetCore.Hosting;
// using Microsoft.Extensions.Hosting;
// using NServiceBus;

// namespace Website
// {
//     public class Program
//     {
//         public static void Main(string[] args)
//         {   
            
//             CreateWebHostBuilder(args).Build().Run();
//         }

//         public static IHostBuilder CreateWebHostBuilder(string[] args) =>
//             Host.CreateDefaultBuilder(args)
//                 .UseNServiceBus(ctx =>
//                 {
//                     const string connectionString = @"Host=localhost;Port=11432;Username=db_user;Password=P@ssw0rd;Database=website_database";
//                     var config = new EndpointConfiguration("Webapp");
//                     //TODO:
//                     // config.ApplyWebsiteConfigurationWithPersistence(connectionString);
//                     return config;
//                 })
//                 .ConfigureWebHostDefaults(webBuilder =>
//                 {
//                     webBuilder.UseStartup<Startup>();
//                 });
//     }
// }
