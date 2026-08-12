using Microsoft.EntityFrameworkCore;
using ITOps.Infrastructure;
using System;

namespace Ticketing.ViewModelComposition
{
    // 🛡️ The centralized ITOps helper for the Ticketing UI slice
    public static class TicketingContextFactory
    {
        public static Func<Data.TicketingContext> Create()
        {
            return () =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<Data.TicketingContext>();
                
                // Pull from the global ITOps component once
                var connectionString = ConnectionStringProvider.GetConnectionString("ticketing-db");
                optionsBuilder.UseNpgsql(connectionString);
                
                return new Data.TicketingContext(optionsBuilder.Options);
            };
        }
    }
}
