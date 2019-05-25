using Microsoft.EntityFrameworkCore;
using Sales.Data.Models;

namespace Sales.Data
{
    public class SalesContext : DbContext
    {
        public static SalesContext Create()
        {
            var db = new SalesContext();
            db.Database.EnsureCreated();

            return db;
        }

        private SalesContext() { }

        public DbSet<Order> Orders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\welcome-to-the-state-machine;Initial Catalog=Sales;Integrated Security=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
    }
}
