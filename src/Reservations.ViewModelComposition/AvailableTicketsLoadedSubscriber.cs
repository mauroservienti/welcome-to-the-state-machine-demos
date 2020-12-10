using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using ServiceComposer.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Ticketing.ViewModelComposition.Events;

namespace Reservations.ViewModelComposition
{
    class AvailableTicketsLoadedSubscriber : ICompositionEventsSubscriber
    {
        [HttpGet("/home/index")]
        public void Subscribe(ICompositionEventsPublisher publisher)
        {
            publisher.Subscribe<AvailableTicketsLoaded>(async (@event, request) =>
            {
                var ids = @event.AvailableTicketsViewModel.Keys.ToArray();
                await using var db = Data.ReservationsContext.Create();
                var availableTickets = await db.AvailableTickets
                    .Where(ticket => ids.Contains(ticket.Id))
                    .ToListAsync();

                IDictionary<int, int> reservedTickets = new Dictionary<int, int>();

                if (request.Cookies.ContainsKey("reservation-id"))
                {
                    var reservationId = new Guid(request.Cookies["reservation-id"]);
                    var reservation = await db.Reservations
                        .Where(r => r.Id == reservationId)
                        .Include(r => r.ReservedTickets)
                        .SingleOrDefaultAsync();

                    if (reservation != null)
                    {
                        reservedTickets = reservation.ReservedTickets
                            .GroupBy(t => t.TicketId)
                            .ToDictionary(g => g.Key, g => g.Count());
                    }
                }

                foreach (var availableTicket in availableTickets)
                {
                    var availableTicketId = (int)availableTicket.Id;
                    var reservedQuantity = reservedTickets.ContainsKey(availableTicketId)
                        ? reservedTickets[availableTicketId]
                        : 0;

                    @event.AvailableTicketsViewModel[availableTicketId].TicketsLeft = availableTicket.TotalTickets - reservedQuantity;
                }
            });
        }
    }
}
