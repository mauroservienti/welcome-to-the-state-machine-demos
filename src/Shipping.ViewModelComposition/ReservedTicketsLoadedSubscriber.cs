using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Reservations.ViewModelComposition.Events;
using ServiceComposer.AspNetCore;
using Shipping.Data;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Shipping.ViewModelComposition
{
    class ReservedTicketsLoadedSubscriber : ICompositionEventsSubscriber
    {
        [HttpGet("/reservations")]
        public void Subscribe(ICompositionEventsPublisher publisher)
        {
            publisher.Subscribe<ReservedTicketsLoaded>((@event, httpRequest) =>
            {
                dynamic shipAtHome = new ExpandoObject();
                shipAtHome.Id = DeliveryOptions.ShipAtHome;
                shipAtHome.Description = "Ship at Home.";

                dynamic collectAtTheVenue = new ExpandoObject();
                collectAtTheVenue.Id = DeliveryOptions.CollectAtTheVenue;
                collectAtTheVenue.Description = "Collect at the Venue.";

                var pageViewModel = httpRequest.GetComposedResponseModel();
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
