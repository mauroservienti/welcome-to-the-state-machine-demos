using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Reservations.ViewModelComposition.Events;
using ServiceComposer.AspNetCore;
using Shipping.Data;
using System;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Shipping.ViewModelComposition
{
    class ReviewReservedTicketsLoadedSubscriber : ISubscribeToCompositionEvents
    {
        public bool Matches(RouteData routeData, string httpVerb, HttpRequest request)
        {
            var controller = (string)routeData.Values["controller"];
            var action = (string)routeData.Values["action"];

            return HttpMethods.IsGet(httpVerb)
                   && controller.ToLowerInvariant() == "reservations"
                   && action.ToLowerInvariant() == "review"
                   && !routeData.Values.ContainsKey("id");
        }

        public void Subscribe(IPublishCompositionEvents publisher)
        {
            publisher.Subscribe<ReservedTicketsLoaded>((requestId, viewModel, @event, douteData, httpRequest) =>
            {
                /*
                 * it's a demo, production code should check for cookie existence
                 */
                var selectedDeliveryOption = (DeliveryOptions)Enum.Parse(typeof(Data.DeliveryOptions), httpRequest.Cookies["reservation-delivery-option-id"]);
                dynamic deliveryOption = new ExpandoObject();
                deliveryOption.Id = selectedDeliveryOption;

                switch (selectedDeliveryOption)
                {
                    case DeliveryOptions.ShipAtHome:
                        deliveryOption.Description = "Ship at Home.";
                        break;
                    case DeliveryOptions.CollectAtTheVenue:
                        deliveryOption.Description = "Collect at the Venue.";
                        break;
                }

                viewModel.DeliveryOption = deliveryOption;

                return Task.CompletedTask;
            });
        }
    }
}
