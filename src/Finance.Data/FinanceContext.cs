using Finance.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Finance.Data
{
    public class FinanceContext : DbContext
    {
        public FinanceContext()
        {
        }

        public FinanceContext(DbContextOptions<FinanceContext> options) : base(options)
        {
        }

        public DbSet<TicketPrice> TicketPrices { get; set; }
        public DbSet<ReservedTicket> ReservedTickets { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // When running via .NET Aspire the connection string is injected as an environment variable
                var connectionString = System.Environment.GetEnvironmentVariable("ConnectionStrings__finance-database")
                    ?? @"Host=localhost;Port=6432;Username=db_user;Password=P@ssw0rd;Database=finance_database";
                optionsBuilder.UseNpgsql(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TicketPrice>().HasData(Seed.TicketPrices());
            modelBuilder.Entity<PaymentMethod>().HasData(Seed.PaymentMethods());
            modelBuilder.Entity<ReservedTicket>();

            base.OnModelCreating(modelBuilder);
        }

        private static class Seed
        {
            internal static PaymentMethod[] PaymentMethods()
            {
                return new[] 
                {
                    new PaymentMethod()
                    {
                        Id = 1,
                        Description = "Master Card (last 4 digits: 5555)"
                    },
                    new PaymentMethod()
                    {
                        Id = 2,
                        Description = "Visa (last 4 digits: 1111)"
                    }
                };
            }

            internal static TicketPrice[] TicketPrices()
            {
                return new[]
                {
                    new TicketPrice()
                    {
                        Id = 1,
                        Price = 96
                    },
                    new TicketPrice()
                    {
                        Id = 2,
                        Price = 0
                    }
                };
            }

        }
    }
}
