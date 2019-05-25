using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using ServiceComposer.AspNetCore;
using System.Linq;
using Ticketing.ViewModelComposition.Events;

namespace Finance.ViewModelComposition
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
            publisher.Subscribe<AvailableTicketsLoaded>(async (requestId, pageViewModel, @event, rd, req) =>
            {
                var ids = @event.AvailableTicketsViewModel.Keys.ToArray();
                using (var db = Data.FinanceContext.Create())
                {
                    var ticketPrices = await db.TicketPrices
                        .Where(ticketPrice => ids.Contains(ticketPrice.Id))
                        .ToListAsync();

                    foreach (var ticketPrice in ticketPrices)
                    {
                        @event.AvailableTicketsViewModel[(int)ticketPrice.Id].TicketPrice = ticketPrice.Price;
                    }
                }
            });
        }
    }
}
