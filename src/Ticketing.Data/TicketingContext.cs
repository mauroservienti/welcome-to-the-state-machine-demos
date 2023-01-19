﻿using Microsoft.EntityFrameworkCore;
using Ticketing.Data.Models;

namespace Ticketing.Data
{
    public class TicketingContext : DbContext
    {
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\welcome-to-the-state-machine;Initial Catalog=Ticketing;Integrated Security=True");
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
