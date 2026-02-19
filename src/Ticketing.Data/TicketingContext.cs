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
                optionsBuilder.UseNpgsql(@"Host=localhost;Port=5432;Username=db_user;Password=P@ssw0rd;Database=ticketing_database");
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
