using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NServiceBus;

namespace Website
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseNServiceBus(ctx =>
                {
                    const string connectionString = @"Host=localhost;Port=11432;Username=db_user;Password=P@ssw0rd;Database=website_database";
                    var config = new EndpointConfiguration("Webapp");
                    config.ApplyWebsiteConfigurationWithPersistence(connectionString);

                    return config;
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
