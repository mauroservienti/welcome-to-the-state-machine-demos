using Microsoft.EntityFrameworkCore;
using Finance.Data.Models;

namespace Finance.Data
{
    public class FinanceContext : DbContext
    {
        public static FinanceContext Create()
        {
            var db = new FinanceContext();
            db.Database.EnsureCreated();

            return db;
        }

        private FinanceContext() { }

        public DbSet<TicketPrice> TicketPrices { get; set; }
        public DbSet<ReservedTicket> ReservedTickets { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\welcome-to-the-state-machine;Initial Catalog=Finance;Integrated Security=True");
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
