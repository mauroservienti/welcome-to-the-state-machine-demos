using Microsoft.Extensions.Hosting;
using NServiceBus;
using System.Threading.Tasks;

// 🌟 Modernized .NET Host Builder: Rips out the obsolete self-hosting loops!
var builder = Host.CreateApplicationBuilder();

builder.AddServiceDefaults();

// Centralized NServiceBus initializer: Points outbox tables to shipping-db
builder.AddNServiceBusEndpoint(
    name: "Shipping.Service", 
    persistenceDbName: "shipping-db", 
    tablePrefix: "shipping");

// Build and execute the standard, non-blocking asynchronous host worker lifecycle
await builder.Build().RunAsync();
