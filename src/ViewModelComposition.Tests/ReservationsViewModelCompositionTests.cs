using Microsoft.EntityFrameworkCore;
using Reservations.Data.Models;
using Reservations.Messages.Commands;
using Reservations.ViewModelComposition;
using Reservations.ViewModelComposition.Events;
using ServiceComposer.AspNetCore;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Ticketing.ViewModelComposition.Events;
using Xunit;

namespace ViewModelComposition.Tests
{
    public class ReservationsViewModelCompositionTests
    {
        static Reservations.Data.ReservationsContext CreateReservationsContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<Reservations.Data.ReservationsContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new Reservations.Data.ReservationsContext(options);
        }

        // TicketsReservationGetHandler

        [Fact]
        public async Task TicketsReservationGetHandler_NoCookie_SetsReservationNull()
        {
            var handler = new TicketsReservationGetHandler(() => CreateReservationsContext(Guid.NewGuid().ToString("N")));
            var (request, vm, _) = TestRequestBuilder.Build();

            await handler.Handle(request);

            Assert.Null(vm.Reservation);
        }

        [Fact]
        public async Task TicketsReservationGetHandler_CookieButNoDbRecord_SetsReservationNull()
        {
            var dbName = Guid.NewGuid().ToString("N");
            var reservationId = Guid.NewGuid();

            var handler = new TicketsReservationGetHandler(() => CreateReservationsContext(dbName));
            var (request, vm, _) = TestRequestBuilder.Build(
                cookies => cookies["reservation-id"] = reservationId.ToString());

            await handler.Handle(request);

            Assert.Null(vm.Reservation);
        }

        [Fact]
        public async Task TicketsReservationGetHandler_ReservationInDb_SetsVmAndRaisesEvent()
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
                        new ReservedTicket { ReservationId = reservationId, TicketId = 1 },
                        new ReservedTicket { ReservationId = reservationId, TicketId = 2 }
                    ]
                });
                await db.SaveChangesAsync();
            }

            var handler = new TicketsReservationGetHandler(() => CreateReservationsContext(dbName));
            var (request, vm, compositionContext) = TestRequestBuilder.Build(
                cookies => cookies["reservation-id"] = reservationId.ToString());

            await handler.Handle(request);

            Assert.NotNull(vm.Reservation);
            Assert.Equal(reservationId, (Guid)vm.Reservation.Id);

            var raisedEvent = compositionContext.RaisedEvents
                .OfType<ReservedTicketsLoaded>()
                .SingleOrDefault();
            Assert.NotNull(raisedEvent);
            // 2 distinct ticket IDs grouped
            Assert.Equal(2, raisedEvent.ReservedTicketsViewModel.Count);

            var reservedTickets = (List<dynamic>)vm.Reservation.ReservedTickets;
            Assert.Equal(2, reservedTickets.Count);
        }

        // ReservationsReservePostHandler

        [Fact]
        public async Task ReservationsReservePostHandler_SendsReserveTicketCommand()
        {
            var reservationId = Guid.NewGuid();
            var ticketId = 7;
            var session = new FakeTransactionalSession();
            var handler = new ReservationsReservePostHandler(session);

            var (request, _, _) = TestRequestBuilder.Build(
                cookies => cookies["reservation-id"] = reservationId.ToString(),
                routeValues => routeValues["id"] = ticketId.ToString());

            await handler.Handle(request);

            var msg = session.SentMessages.OfType<ReserveTicket>().SingleOrDefault();
            Assert.NotNull(msg);
            Assert.Equal(reservationId, msg.ReservationId);
            Assert.Equal(ticketId, msg.TicketId);
        }

        // ReservationsCheckoutPostHandler

        [Fact]
        public async Task ReservationsCheckoutPostHandler_SendsCheckoutReservationCommand()
        {
            var reservationId = Guid.NewGuid();
            var session = new FakeTransactionalSession();
            var handler = new ReservationsCheckoutPostHandler(session);

            var (request, _, _) = TestRequestBuilder.Build(
                cookies => cookies["reservation-id"] = reservationId.ToString());

            await handler.Handle(request);

            var msg = session.SentMessages.OfType<CheckoutReservation>().SingleOrDefault();
            Assert.NotNull(msg);
            Assert.Equal(reservationId, msg.ReservationId);
        }

        // ReservationsCheckedoutGetHandler

        [Fact]
        public async Task ReservationsCheckedoutGetHandler_ExpiresCookie()
        {
            var handler = new ReservationsCheckedoutGetHandler();
            var (request, _, _) = TestRequestBuilder.Build();

            await handler.Handle(request);

            var setCookie = request.HttpContext.Response.Headers["Set-Cookie"].ToString();
            Assert.Contains("reservation-id=", setCookie);
            Assert.Contains("expires=", setCookie, StringComparison.OrdinalIgnoreCase);
        }

        // AvailableTicketsLoadedSubscriber

        [Fact]
        public async Task AvailableTicketsLoadedSubscriber_NoReservationCookie_SetsTicketsLeftToTotal()
        {
            var dbName = Guid.NewGuid().ToString("N");
            await using (var db = CreateReservationsContext(dbName))
            {
                db.AvailableTickets.Add(new Reservations.Data.Models.AvailableTickets { Id = 1, TotalTickets = 100 });
                await db.SaveChangesAsync();
            }

            dynamic ticketVm = new ExpandoObject();
            var availableVm = new Dictionary<int, dynamic> { [1] = ticketVm };
            var @event = new AvailableTicketsLoaded { AvailableTicketsViewModel = availableVm };

            CompositionEventHandler<AvailableTicketsLoaded>? capturedHandler = null;
            var publisher = new FakeCompositionEventsPublisher<AvailableTicketsLoaded>(h => capturedHandler = h);

            var subscriber = new AvailableTicketsLoadedSubscriber(() => CreateReservationsContext(dbName));
            subscriber.Subscribe(publisher);

            var (request, _, _) = TestRequestBuilder.Build();
            await capturedHandler!(@event, request);

            Assert.Equal(100, (int)ticketVm.TicketsLeft);
        }

        [Fact]
        public async Task AvailableTicketsLoadedSubscriber_WithReservation_SubtractsReservedFromTotal()
        {
            var dbName = Guid.NewGuid().ToString("N");
            var reservationId = Guid.NewGuid();

            await using (var db = CreateReservationsContext(dbName))
            {
                db.AvailableTickets.Add(new Reservations.Data.Models.AvailableTickets { Id = 1, TotalTickets = 100 });
                db.Reservations.Add(new Reservation
                {
                    Id = reservationId,
                    ReservedTickets =
                    [
                        new ReservedTicket { ReservationId = reservationId, TicketId = 1 },
                        new ReservedTicket { ReservationId = reservationId, TicketId = 1 }
                    ]
                });
                await db.SaveChangesAsync();
            }

            dynamic ticketVm = new ExpandoObject();
            var availableVm = new Dictionary<int, dynamic> { [1] = ticketVm };
            var @event = new AvailableTicketsLoaded { AvailableTicketsViewModel = availableVm };

            CompositionEventHandler<AvailableTicketsLoaded>? capturedHandler = null;
            var publisher = new FakeCompositionEventsPublisher<AvailableTicketsLoaded>(h => capturedHandler = h);

            var subscriber = new AvailableTicketsLoadedSubscriber(() => CreateReservationsContext(dbName));
            subscriber.Subscribe(publisher);

            var (request, _, _) = TestRequestBuilder.Build(
                cookies => cookies["reservation-id"] = reservationId.ToString());
            await capturedHandler!(@event, request);

            Assert.Equal(98, (int)ticketVm.TicketsLeft);
        }
    }
}
