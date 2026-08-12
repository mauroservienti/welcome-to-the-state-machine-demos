using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using NServiceBus.Persistence.Sql; // 🌟 Resolves your handler's compilation error!
using Finance.Data;
using Finance.PaymentGateway.Messages; // 🌟 Access card message definitions
using System;

var builder = Host.CreateApplicationBuilder();

builder.AddServiceDefaults();

// 1. Centralized NServiceBus Endpoint initialization + Custom Routing Lambda!
builder.AddNServiceBusEndpoint(
    name: "Finance.Service", 
    persistenceDbName: "finance-db", 
    tablePrefix: "finance",
    configureEndpoint: (endpointConfiguration, routing) =>
    {
        // 🚀 MAURO'S ROUTING HOOK PORTED: Explicitly tell NServiceBus where to send card commands!
        routing.RouteToEndpoint(typeof(AuthorizeCard), "Finance.PaymentGateway");
        routing.RouteToEndpoint(typeof(ReleaseCardAuthorization), "Finance.PaymentGateway");
        routing.RouteToEndpoint(typeof(ChargeCard), "Finance.PaymentGateway");
    });

// 2. THE DOCUMENTATION PATTERN: Wire the shared transaction Unit of Work [INDEX]!
builder.Services.AddScoped<FinanceContext>(provider =>
{
    var session = provider.GetRequiredService<ISqlStorageSession>();

    var optionsBuilder = new DbContextOptionsBuilder<FinanceContext>();
    optionsBuilder.UseNpgsql(session.Connection); 

    var context = new FinanceContext(optionsBuilder.Options);

    context.Database.UseTransaction(session.Transaction);
    session.OnSaveChanges((s, token) => context.SaveChangesAsync(token));

    return context;
});

await builder.Build().RunAsync();





// using Finance.PaymentGateway.Messages;
// using NServiceBus;
// using System;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;

// namespace Finance.Service
// {
//     class Program
//     {
//         static void Main(string[] args)
//         {
//             var serviceName = typeof(Program).Namespace;
//             Console.Title = serviceName;

//             CreateHostBuilder(serviceName, args).Build().Run();
//         }

//         static IHostBuilder CreateHostBuilder(string serviceName, string[] args)
//         {
//             var builder = Host.CreateDefaultBuilder(args)
//                 .ConfigureLogging((ctx, logging) =>
//                 {
//                     logging.AddConfiguration(ctx.Configuration.GetSection("Logging"));
//                     logging.AddConsole();
//                 })
//                 .UseNServiceBus(ctx =>
//                 {
//                     //@"Host=localhost;Port=6432;Username=db_user;Password=P@ssw0rd;Database=finance_database"
//                     const string connectionString = @"Host=localhost;Port=6432;Username=db_user;Password=P@ssw0rd;Database=finance_database";
                    
//                     //const string connectionString = @"Host=localhost;Port=7432;Username=db_user;Password=P@ssw0rd;Database=finance_service_database";
//                     var config = new EndpointConfiguration(serviceName);
//                     config.ApplyCommonConfigurationWithPersistence(connectionString, tablePrefix:"Finance", configureRouting: routing =>
//                     {
//                         routing.RouteToEndpoint(typeof(AuthorizeCard), "Finance.PaymentGateway");
//                         routing.RouteToEndpoint(typeof(ReleaseCardAuthorization), "Finance.PaymentGateway");
//                         routing.RouteToEndpoint(typeof(ChargeCard), "Finance.PaymentGateway");
//                     });


//     // FORCE THE RUNTIME ASSEMBLY TO INITIALIZE THE SAGA AUDIT INTERCEPTOR PIPELINE
//     config.AuditSagaStateChanges(
//         serviceControlQueue: "Particular.ServiceControl"
//     );

//                     return config;
//                 });

//             return builder;
//         }
//     }
// }
