using Shipping.Data;
using System;

namespace Shipping.Messages.Commands
{
    public class InitializeReservationShippingPolicy
    {
        public Guid ReservationId { get; set; }
        public DeliveryOptions DeliveryOption { get; set; }
    }
}
