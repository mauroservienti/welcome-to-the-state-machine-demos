using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Reservations.ViewModelComposition.Events;
using ServiceComposer.AspNetCore;
using System.Linq;

namespace Ticketing.ViewModelComposition
{
    class ReservedTicketsLoadedSubscriber : ISubscribeToCompositionEvents
    {
        public bool Matches(RouteData routeData, string httpVerb, HttpRequest request)
        {
            var controller = (string)routeData.Values["controller"];
            var action = (string)routeData.Values["action"];

            return HttpMethods.IsGet(httpVerb)
                   && controller.ToLowerInvariant() == "reservations"
                   && action.ToLowerInvariant() == "index"
                   && !routeData.Values.ContainsKey("id");
        }

        public void Subscribe(IPublishCompositionEvents publisher)
        {
            publisher.Subscribe<ReservedTicketsLoaded>(async (requestId, pageViewModel, @event, rd, req) =>
            {
                var ids = @event.ReservedTicketsViewModel.Keys.ToArray();
                using (var db = Data.TicketingContext.Create())
                {
                    var reservedTickets = await db.Tickets
                        .Where(ticket => ids.Contains(ticket.Id))
                        .ToListAsync();

                    foreach (var ticket in reservedTickets)
                    {
                        @event.ReservedTicketsViewModel[(int)ticket.Id].TicketDescription = ticket.Description;
                    }
                }
            });
        }
    }
}
