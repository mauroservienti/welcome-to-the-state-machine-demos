using Microsoft.EntityFrameworkCore;
using Ticketing.Data.Models;

namespace Ticketing.Data
{
    public class TicketingContext : DbContext
    {
        public TicketingContext()
        {
        }

        public TicketingContext(DbContextOptions<TicketingContext> options) : base(options)
        {
        }

        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // 🪓 FAIL HARD: The context refuses to guess or fall back to local ports!
                throw new System.InvalidOperationException(
                    "TicketingContext Critical Error: This DbContext was instantiated without any active configurations. " +
                    "Ensure the host project registers this context via Dependency Injection, or that the calling " +
                    "view model composition handler explicitly provides a configured DbContextOptions block.");
                
                //optionsBuilder.UseNpgsql(@"Host=localhost;Port=5432;Username=db_user;Password=P@ssw0rd;Database=ticketing_database");
                //optionsBuilder.UseNpgsql(@"Host=localhost;Port=5499;Username=postgres;Password=P@ssw0rd;Database=ticketing-db;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ticket>().HasData(Seed.Tickets());

            base.OnModelCreating(modelBuilder);
        }

        private static class Seed
        {
            internal static Ticket[] Tickets()
            {
                return new[]
                {
                    new Ticket()
                    {
                        Id = 1,
                        Description = "Monsters of Rock, Modena Italy - 1991"
                    },
                    new Ticket()
                    {
                        Id = 2,
                        Description = "Pink Floyd, Venice Italy - 1989",
                    }
                };
            }

        }
    }
}
