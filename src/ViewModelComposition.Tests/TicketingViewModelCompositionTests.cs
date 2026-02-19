using Microsoft.EntityFrameworkCore;
using Reservations.ViewModelComposition.Events;
using ServiceComposer.AspNetCore;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Ticketing.ViewModelComposition;
using Xunit;

namespace ViewModelComposition.Tests
{
    public class TicketingViewModelCompositionTests
    {
        static Ticketing.Data.TicketingContext CreateTicketingContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<Ticketing.Data.TicketingContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new Ticketing.Data.TicketingContext(options);
        }

        // AvailableTicketsGetHandler

        [Fact]
        public async Task AvailableTicketsGetHandler_LoadsAllTickets_SetsVmAndRaisesEvent()
        {
            var dbName = Guid.NewGuid().ToString("N");
            await using (var db = CreateTicketingContext(dbName))
            {
                db.Tickets.AddRange(
                    new Ticketing.Data.Models.Ticket { Id = 1, Description = "Ticket A" },
                    new Ticketing.Data.Models.Ticket { Id = 2, Description = "Ticket B" });
                await db.SaveChangesAsync();
            }

            var handler = new AvailableTicketsGetHandler(() => CreateTicketingContext(dbName));
            var (request, vm, compositionContext) = TestRequestBuilder.Build();

            await handler.Handle(request);

            var raisedEvent = compositionContext.RaisedEvents
                .OfType<Ticketing.ViewModelComposition.Events.AvailableTicketsLoaded>()
                .SingleOrDefault();
            Assert.NotNull(raisedEvent);
            Assert.Equal(2, raisedEvent.AvailableTicketsViewModel.Count);

            var tickets = (List<dynamic>)vm.AvailableTickets;
            Assert.Equal(2, tickets.Count);
        }

        [Fact]
        public async Task AvailableTicketsGetHandler_EmptyDb_SetsEmptyVm()
        {
            var dbName = Guid.NewGuid().ToString("N");
            var handler = new AvailableTicketsGetHandler(() => CreateTicketingContext(dbName));
            var (request, vm, _) = TestRequestBuilder.Build();

            await handler.Handle(request);

            var tickets = (List<dynamic>)vm.AvailableTickets;
            Assert.Empty(tickets);
        }

        // ReservedTicketsLoadedSubscriber

        [Fact]
        public async Task ReservedTicketsLoadedSubscriber_EnrichesWithDescription()
        {
            var dbName = Guid.NewGuid().ToString("N");
            await using (var db = CreateTicketingContext(dbName))
            {
                db.Tickets.Add(new Ticketing.Data.Models.Ticket { Id = 3, Description = "Ticket C" });
                await db.SaveChangesAsync();
            }

            dynamic ticketVm = new ExpandoObject();
            var reservedTicketsVm = new Dictionary<int, dynamic> { [3] = ticketVm };
            dynamic reservation = new ExpandoObject();
            reservation.Id = Guid.NewGuid();

            var @event = new ReservedTicketsLoaded
            {
                ReservedTicketsViewModel = reservedTicketsVm,
                Reservation = reservation
            };

            CompositionEventHandler<ReservedTicketsLoaded>? capturedHandler = null;
            var publisher = new FakeCompositionEventsPublisher<ReservedTicketsLoaded>(h => capturedHandler = h);

            var subscriber = new ReservedTicketsLoadedSubscriber(() => CreateTicketingContext(dbName));
            subscriber.Subscribe(publisher);

            var (request, _, _) = TestRequestBuilder.Build();
            await capturedHandler!(@event, request);

            Assert.Equal("Ticket C", (string)ticketVm.TicketDescription);
        }

        [Fact]
        public async Task ReservedTicketsLoadedSubscriber_UnknownTicketId_DoesNotThrow()
        {
            var dbName = Guid.NewGuid().ToString("N");
            dynamic ticketVm = new ExpandoObject();
            var reservedTicketsVm = new Dictionary<int, dynamic> { [999] = ticketVm };
            dynamic reservation = new ExpandoObject();
            reservation.Id = Guid.NewGuid();

            var @event = new ReservedTicketsLoaded
            {
                ReservedTicketsViewModel = reservedTicketsVm,
                Reservation = reservation
            };

            CompositionEventHandler<ReservedTicketsLoaded>? capturedHandler = null;
            var publisher = new FakeCompositionEventsPublisher<ReservedTicketsLoaded>(h => capturedHandler = h);

            var subscriber = new ReservedTicketsLoadedSubscriber(() => CreateTicketingContext(dbName));
            subscriber.Subscribe(publisher);

            var (request, _, _) = TestRequestBuilder.Build();
            // Should not throw even if ticket ID doesn't exist in DB
            await capturedHandler!(@event, request);
        }
    }
}
