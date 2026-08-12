using Microsoft.EntityFrameworkCore;
using ITOps.Infrastructure;
using System;

namespace Reservations.ViewModelComposition
{
    public static class ReservationsContextFactory
    {
        public static Func<Data.ReservationsContext> Create()
        {
            return () =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<Data.ReservationsContext>();
                
                // 🐋 Pulls the specific reservations database connection key
                var connectionString = ConnectionStringProvider.GetConnectionString("reservation-db");
                optionsBuilder.UseNpgsql(connectionString);
                
                return new Data.ReservationsContext(optionsBuilder.Options);
            };
        }
    }
}
