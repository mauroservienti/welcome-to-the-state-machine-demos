using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Reservations.ViewModelComposition.Events;
using ServiceComposer.AspNetCore;
using Shipping.Data;
using System;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Shipping.ViewModelComposition
{
    class ReviewReservedTicketsLoadedSubscriber : ICompositionEventsSubscriber
    {
        [HttpGet("/reservations/review")]
        public void Subscribe(ICompositionEventsPublisher publisher)
        {
            publisher.Subscribe<ReservedTicketsLoaded>((@event, httpRequest) =>
            {
                /*
                 * it's a demo, production code should check for cookie existence
                 */
                var selectedDeliveryOption = (DeliveryOptions)Enum.Parse(typeof(Data.DeliveryOptions), httpRequest.Cookies["reservation-delivery-option-id"]);
                dynamic deliveryOption = new ExpandoObject();
                deliveryOption.Id = selectedDeliveryOption;

                deliveryOption.Description = selectedDeliveryOption switch
                {
                    DeliveryOptions.ShipAtHome => "Ship at Home.",
                    DeliveryOptions.CollectAtTheVenue => "Collect at the Venue.",
                    _ => deliveryOption.Description
                };

                var viewModel = httpRequest.GetComposedResponseModel();
                viewModel.DeliveryOption = deliveryOption;

                return Task.CompletedTask;
            });
        }
    }
}
