using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using NpgsqlTypes;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using Reservations.Data;

var builder = Host.CreateApplicationBuilder();

builder.AddServiceDefaults()
    .AddNServiceBusEndpoint(
        name: "Reservations.Service", 
        persistenceDbName: "reservation-db",
        tablePrefix: "reservation",
        enableOutbox: true,
        enableTransactionalSession: false
    );

    builder.Services.AddScoped<ReservationsContext>(provider =>
    {
        // A. Borrow NServiceBus's active storage session block for the incoming message thread
        var session = provider.GetRequiredService<ISqlStorageSession>();

        var optionsBuilder = new DbContextOptionsBuilder<ReservationsContext>();
        optionsBuilder.UseNpgsql(session.Connection); // 🌟 Uses NServiceBus's active open socket!

        var context = new ReservationsContext(optionsBuilder.Options);

        // B. Enlist Entity Framework inside NServiceBus's active database transaction block
        context.Database.UseTransaction(session.Transaction); // 🌟 Binds them to the same atomic transaction!

        // C. Automatically flush EF changes right before NServiceBus commits the transaction
        session.OnSaveChanges((s, token) => context.SaveChangesAsync(token));

        return context;
    });

await builder.Build().RunAsync();





// using NServiceBus;
// using System;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;

// namespace Reservations.Service
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
//                     const string connectionString = @"Host=localhost;Port=9432;Username=db_user;Password=P@ssw0rd;Database=reservations_service_database";
//                     var config = new EndpointConfiguration(serviceName);
                    
//                     //TODO:
//                     //config.ApplyCommonConfigurationWithPersistence(connectionString, tablePrefix: "reservations");

//                     return config;
//                 });

//             return builder;
//         }
//     }
// }
