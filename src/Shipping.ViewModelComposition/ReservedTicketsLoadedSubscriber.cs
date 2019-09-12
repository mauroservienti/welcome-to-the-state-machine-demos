using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Reservations.ViewModelComposition.Events;
using ServiceComposer.AspNetCore;
using Shipping.Data;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace Shipping.ViewModelComposition
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
            publisher.Subscribe<ReservedTicketsLoaded>((requestId, pageViewModel, @event, rd, req) =>
            {
                dynamic shipAtHome = new ExpandoObject();
                shipAtHome.Id = DeliveryOptions.ShipAtHome;
                shipAtHome.Description = "Ship at Home.";

                dynamic collectAtTheVenue = new ExpandoObject();
                collectAtTheVenue.Id = DeliveryOptions.CollectAtTheVenue;
                collectAtTheVenue.Description = "Collect at the Venue.";

                pageViewModel.DeliveryOptions = new List<dynamic>()
                {
                    shipAtHome,
                    collectAtTheVenue
                };

                return Task.CompletedTask;
            });
        }
    }
}
