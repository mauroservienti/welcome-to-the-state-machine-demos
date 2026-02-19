using Finance.Data;
using Finance.Messages.Commands;
using Finance.Service.Handlers;
using Microsoft.EntityFrameworkCore;
using NServiceBus.Testing;
using Reservations.Data;
using Reservations.Data.Models;
using Reservations.Messages.Commands;
using Reservations.Messages.Events;
using Reservations.Service.Handlers;
using Reservations.Service.Messages;
using Shipping.Messages.Commands;
using Shipping.Service.Handlers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Policies.Tests
{
    public class ServiceHandlersTests
    {
        static ReservationsContext CreateReservationsContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ReservationsContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            return new ReservationsContext(options);
        }

        static FinanceContext CreateFinanceContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<FinanceContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            return new FinanceContext(options);
        }

        [Fact]
        public async Task Handle_MarkTicketAsReserved_AddsTicketToReservation()
        {
            var dbName = Guid.NewGuid().ToString("N");
            var reservationId = Guid.NewGuid();
            var ticketId = 42;
            var handler = new MarkTicketAsReservedHandler(() => CreateReservationsContext(dbName));
            var context = new TestableMessageHandlerContext();

            await handler.Handle(new MarkTicketAsReserved { ReservationId = reservationId, TicketId = ticketId }, context);

            await using var db = CreateReservationsContext(dbName);
            Assert.Single(db.Reservations);
            var reservation = await db.Reservations.Include(r => r.ReservedTickets).SingleAsync();

            Assert.Equal(reservationId, reservation.Id);
            Assert.Single(reservation.ReservedTickets);
            Assert.Equal(ticketId, reservation.ReservedTickets[0].TicketId);
        }

        [Fact]
        public async Task Handle_MarkTicketAsReserved_DuplicateMessage_DoesNotAddDuplicateTicket()
        {
            var dbName = Guid.NewGuid().ToString("N");
            var reservationId = Guid.NewGuid();
            var ticketId = 42;
            var handler = new MarkTicketAsReservedHandler(() => CreateReservationsContext(dbName));
            var context = new TestableMessageHandlerContext();
            var message = new MarkTicketAsReserved { ReservationId = reservationId, TicketId = ticketId };

            await handler.Handle(message, context);
            await handler.Handle(message, context);

            await using var db = CreateReservationsContext(dbName);
            var reservation = await db.Reservations.Include(r => r.ReservedTickets).SingleAsync();
            Assert.Single(reservation.ReservedTickets);
        }

        [Fact]
        public async Task Handle_CheckoutReservation_PublishesReservationCheckedout_AndRemovesReservation()
        {
            var dbName = Guid.NewGuid().ToString("N");
            var reservationId = Guid.NewGuid();

            await using (var db = CreateReservationsContext(dbName))
            {
                db.Reservations.Add(new Reservation
                {
                    Id = reservationId,
                    ReservedTickets =
                    [
                        new ReservedTicket { ReservationId = reservationId, TicketId = 1 },
                        new ReservedTicket { ReservationId = reservationId, TicketId = 2 }
                    ]
                });
                await db.SaveChangesAsync();
            }

            var handler = new CheckoutReservationHandler(() => CreateReservationsContext(dbName));
            var context = new TestableMessageHandlerContext();

            await handler.Handle(new CheckoutReservation { ReservationId = reservationId }, context);

            var published = context.PublishedMessages.Select(m => m.Message).OfType<ReservationCheckedout>().Single();
            Assert.Equal(reservationId, published.ReservationId);
            Assert.Equal(new[] { 1, 2 }, published.Tickets.OrderBy(t => t));

            await using var verifyDb = CreateReservationsContext(dbName);
            Assert.Empty(verifyDb.Reservations);
        }

        [Fact]
        public async Task Handle_CheckoutReservation_MissingReservation_DoesNotPublishMessages()
        {
            var handler = new CheckoutReservationHandler(() => CreateReservationsContext(Guid.NewGuid().ToString("N")));
            var context = new TestableMessageHandlerContext();

            await handler.Handle(new CheckoutReservation { ReservationId = Guid.NewGuid() }, context);

            Assert.Empty(context.PublishedMessages);
        }

        [Fact]
        public async Task Handle_ReservationCheckedout_PublishesOrderCreated_AndStoresAggregatedTickets()
        {
            var dbName = Guid.NewGuid().ToString("N");
            var reservationId = Guid.NewGuid();
            var handler = new ReservationCheckedoutHandler(() => CreateReservationsContext(dbName));
            var context = new TestableMessageHandlerContext();

            await handler.Handle(new TestableReservationCheckedout { ReservationId = reservationId, Tickets = [1, 1, 2] }, context);

            var published = context.PublishedMessages.Select(m => m.Message).OfType<OrderCreated>().Single();
            Assert.Equal(reservationId, published.ReservationId);

            await using var db = CreateReservationsContext(dbName);
            var order = await db.Orders.Include(o => o.OrderedTickets).SingleAsync();
            Assert.Equal(published.OrderId, order.Id);
            Assert.Equal(reservationId, order.ReservationId);
            Assert.Collection(
                order.OrderedTickets.OrderBy(t => t.TicketId),
                ticket =>
                {
                    Assert.Equal(1, ticket.TicketId);
                    Assert.Equal(2, ticket.Quantity);
                },
                ticket =>
                {
                    Assert.Equal(2, ticket.TicketId);
                    Assert.Equal(1, ticket.Quantity);
                });
        }

        [Fact]
        public async Task Handle_StoreReservedTicket_AddsReservedTicket()
        {
            var dbName = Guid.NewGuid().ToString("N");
            var reservationId = Guid.NewGuid();
            var ticketId = 7;
            var handler = new StoreReservedTicketHandler(() => CreateFinanceContext(dbName));
            var context = new TestableMessageHandlerContext();

            await handler.Handle(new StoreReservedTicket { ReservationId = reservationId, TicketId = ticketId }, context);

            await using var db = CreateFinanceContext(dbName);
            var storedTicket = await db.ReservedTickets.SingleAsync();
            Assert.Equal(reservationId, storedTicket.ReservationId);
            Assert.Equal(ticketId, storedTicket.TicketId);
        }

        [Fact]
        public async Task Handle_StoreReservedTicket_DuplicateMessage_DoesNotAddDuplicateTicket()
        {
            var dbName = Guid.NewGuid().ToString("N");
            var reservationId = Guid.NewGuid();
            var ticketId = 7;
            var handler = new StoreReservedTicketHandler(() => CreateFinanceContext(dbName));
            var context = new TestableMessageHandlerContext();
            var message = new StoreReservedTicket { ReservationId = reservationId, TicketId = ticketId };

            await handler.Handle(message, context);
            await handler.Handle(message, context);

            await using var db = CreateFinanceContext(dbName);
            Assert.Single(db.ReservedTickets);
        }

        [Fact]
        public async Task Handle_StoreReservationForVenueDelivery_CompletesWithoutReply()
        {
            var handler = new StoreReservationForVenueDeliveryHandler();
            var context = new TestableMessageHandlerContext();

            await handler.Handle(new StoreReservationForVenueDelivery { ReservationId = Guid.NewGuid(), OrderId = Guid.NewGuid() }, context);

            Assert.Empty(context.RepliedMessages);
            Assert.Empty(context.SentMessages);
            Assert.Empty(context.PublishedMessages);
        }

        class TestableReservationCheckedout : IReservationCheckedout
        {
            public Guid ReservationId { get; set; }
            public int[] Tickets { get; set; } = [];
        }
    }
}
