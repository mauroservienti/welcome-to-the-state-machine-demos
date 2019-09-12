using NServiceBus;
using Shipping.Messages.Commands;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace Shipping.Service.Handlers
{
    class StoreReservationForVenueDeliveryHandler : IHandleMessages<StoreReservationForVenueDelivery>
    {
        public Task Handle(StoreReservationForVenueDelivery message, IMessageHandlerContext context)
        {
            /*
             * This empty handler has the only reason to avoid
             * that the StoreReservationForVenueDelivery fails
             * because no handlers can be found.
             */
            Console.WriteLine($"Venue delivery batch for {message.OrderId} accepted.", Color.Green);

            return Task.CompletedTask;
        }
    }
}
