using Microsoft.EntityFrameworkCore;
using Reservations.ViewModelComposition.Events;
using ServiceComposer.AspNetCore;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Ticketing.ViewModelComposition
{
    class ReservedTicketsLoadedSubscriber : ICompositionEventsSubscriber
    {
        [HttpGet("/reservations")]
        [HttpGet("/reservations/review")]
        public void Subscribe(ICompositionEventsPublisher publisher)
        {
            publisher.Subscribe<ReservedTicketsLoaded>(async (@event, request) =>
            {
                var ids = @event.ReservedTicketsViewModel.Keys.ToArray();
                await using var db = new Data.TicketingContext();
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
