using Microsoft.EntityFrameworkCore;
using Reservations.ViewModelComposition.Events;
using ServiceComposer.AspNetCore;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Ticketing.ViewModelComposition
{
    class ReservedTicketsLoadedSubscriber : ICompositionEventsSubscriber
    {
        readonly Func<Data.TicketingContext> contextFactory;

        public ReservedTicketsLoadedSubscriber() : this(() => new Data.TicketingContext())
        {
        }

        internal ReservedTicketsLoadedSubscriber(Func<Data.TicketingContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        [HttpGet("/reservations")]
        [HttpGet("/reservations/review")]
        public void Subscribe(ICompositionEventsPublisher publisher)
        {
            publisher.Subscribe<ReservedTicketsLoaded>(async (@event, request) =>
            {
                var ids = @event.ReservedTicketsViewModel.Keys.ToArray();
                await using var db = contextFactory();
                var reservedTickets = await db.Tickets
                    .Where(ticket => ids.Contains(ticket.Id))
                    .ToListAsync();

                foreach (var ticket in reservedTickets)
                {
                    @event.ReservedTicketsViewModel[(int)ticket.Id].TicketDescription = ticket.Description;
                }
            });
        }
    }
}
