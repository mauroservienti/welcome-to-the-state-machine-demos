using Microsoft.Extensions.Hosting;
using NServiceBus;
using System.Threading.Tasks;

// 🌟 Clear out Mauro's old obsolete loops and align with Aspire Defaults!
var builder = Host.CreateApplicationBuilder();

builder.AddServiceDefaults();

// Centralized NServiceBus initializer: Points outbox tables to finance-db
builder.AddNServiceBusEndpoint(
    name: "Finance.PaymentGateway", 
    persistenceDbName: "finance-db", 
    tablePrefix: "paymentgateway");

// Build and execute the standard, non-blocking asynchronous host lifecycle worker
await builder.Build().RunAsync();



// using NServiceBus;
// using System;
// using System.Threading.Tasks;

// namespace Finance.PaymentGateway
// {
//     class Program
//     {
//         static async Task Main(string[] args)
//         {
//             var serviceName = typeof(Program).Namespace;
//             Console.Title = serviceName;

//             const string connectionString = @"Host=localhost;Port=7432;Username=db_user;Password=P@ssw0rd;Database=finance_service_database";
//             var config = new EndpointConfiguration(serviceName);
//             config.ApplyCommonConfigurationWithPersistence(connectionString, tablePrefix:"FinPayGate");
            
//             var endpointInstance = await Endpoint.Start(config);

//             Console.WriteLine($"{serviceName} started. Press any key to stop.");
//             Console.ReadLine();

//             await endpointInstance.Stop();
//         }
//     }
// }
