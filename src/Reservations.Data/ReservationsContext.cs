﻿using Microsoft.EntityFrameworkCore;
using Reservations.Data.Models;

namespace Reservations.Data
{
    public class ReservationsContext : DbContext
    {
        public DbSet<Reservation> Reservations { get; set; }

        public DbSet<AvailableTickets> AvailableTickets { get; set; }

        public DbSet<Order> Orders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\welcome-to-the-state-machine;Initial Catalog=Reservations;Integrated Security=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AvailableTickets>().HasData(Seed.AvailableTickets());

            var reservedTicketEntity = modelBuilder.Entity<ReservedTicket>();
            var reservationEntity = modelBuilder.Entity<Reservation>();

            reservedTicketEntity
                .HasOne<Reservation>()
                .WithMany(r => r.ReservedTickets)
                .IsRequired()
                .HasForeignKey(so => so.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            var orderedTicketEntity = modelBuilder.Entity<OrderedTicket>();
            var orderEntity = modelBuilder.Entity<Order>();

            orderedTicketEntity
                .HasOne<Order>()
                .WithMany(r => r.OrderedTickets)
                .IsRequired()
                .HasForeignKey(so => so.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }

        private static class Seed
        {
            internal static AvailableTickets[] AvailableTickets()
            {
                return new[]
                {
                    new AvailableTickets()
                    {
                        Id = 1,
                        TotalTickets= 100
                    },
                    new AvailableTickets()
                    {
                        Id = 2,
                        TotalTickets= 200
                    }
                };
            }

        }
    }
}
