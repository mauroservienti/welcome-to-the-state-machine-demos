using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;

namespace Website
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.AddServiceDefaults();

            // Connection strings are injected by .NET Aspire, defaults target the dev container
            var connectionString = builder.Configuration.GetConnectionString("website-database")
                ?? @"Host=localhost;Port=11432;Username=db_user;Password=P@ssw0rd;Database=website_database";

            var config = new EndpointConfiguration("Webapp");
            config.ApplyWebsiteConfigurationWithPersistence(connectionString, builder.Configuration);
            builder.Services.AddNServiceBusEndpoint(config);

            var startup = new Startup();
            startup.ConfigureServices(builder.Services);

            var app = builder.Build();
            startup.Configure(app);
            app.MapDefaultEndpoints();

            app.Run();
        }
    }
}
