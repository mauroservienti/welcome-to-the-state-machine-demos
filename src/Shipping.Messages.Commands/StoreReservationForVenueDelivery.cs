using System;
using System.Collections.Generic;
using System.Text;

namespace Shipping.Messages.Commands
{
    public class StoreReservationForVenueDelivery
    {
        public Guid OrderId { get; set; }
        public Guid ReservationId { get; set; }
    }
}
