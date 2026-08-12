using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Finance.Data;
using Reservations.Data;
using Ticketing.Data;

var builder = Host.CreateApplicationBuilder(args);

// Register the contexts matching the AppHost references
builder.AddNpgsqlDbContext<TicketingContext>("ticketing-db");
builder.AddNpgsqlDbContext<ReservationsContext>("reservation-db");
builder.AddNpgsqlDbContext<FinanceContext>("finance-db");

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    // 1. Initialize Reservations
    var resContext = scope.ServiceProvider.GetRequiredService<ReservationsContext>();
    resContext.Database.EnsureCreated(); // Works perfectly because NServiceBus hasn't started yet!

    // 2. Initialize Finance
    var finContext = scope.ServiceProvider.GetRequiredService<FinanceContext>();
    finContext.Database.EnsureCreated();

    // 3. Initialize Ticketing
    var tickContext = scope.ServiceProvider.GetRequiredService<TicketingContext>();
    tickContext.Database.EnsureCreated();
}

return; // Shuts down smoothly so Aspire knows it can boot the main microservices
