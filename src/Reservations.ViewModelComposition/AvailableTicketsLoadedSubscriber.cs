using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using ServiceComposer.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Ticketing.ViewModelComposition.Events;

namespace Reservations.ViewModelComposition
{
    class AvailableTicketsLoadedSubscriber : ISubscribeToCompositionEvents
    {
        public bool Matches(RouteData routeData, string httpVerb, HttpRequest request)
        {
            var controller = (string)routeData.Values["controller"];
            var action = (string)routeData.Values["action"];

            return HttpMethods.IsGet(httpVerb)
                   && controller.ToLowerInvariant() == "home"
                   && action.ToLowerInvariant() == "index"
                   && !routeData.Values.ContainsKey("id");
        }

        public void Subscribe(IPublishCompositionEvents publisher)
        {
            publisher.Subscribe<AvailableTicketsLoaded>(async (requestId, viewModel, @event, routeData, httpRequest) =>
            {
                var ids = @event.AvailableTicketsViewModel.Keys.ToArray();
                using (var db = Data.ReservationsContext.Create())
                {
                    var availableTickets = await db.AvailableTickets
                        .Where(ticket => ids.Contains(ticket.Id))
                        .ToListAsync();

                    IDictionary<int, int> reservedTickets = new Dictionary<int, int>();

                    if (httpRequest.Cookies.ContainsKey("reservation-id"))
                    {
                        var reservationId = new Guid(httpRequest.Cookies["reservation-id"]);
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
                        var avialableTicketId = (int)availableTicket.Id;
                        var reservedQuantity = reservedTickets.ContainsKey(avialableTicketId)
                            ? reservedTickets[avialableTicketId]
                            : 0;

                        @event.AvailableTicketsViewModel[avialableTicketId].TicketsLeft = availableTicket.TotalTickets - reservedQuantity;
                    }
                }
            });
        }
    }
}
