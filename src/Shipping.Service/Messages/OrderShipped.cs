using Shipping.Messages.Events;
using System;

namespace Shipping.Service.Messages
{
    class OrderShipped : IOrderShipped
    {
        public Guid ReservationId { get; set; }

        public Guid OrderId { get; set; }

        public Guid ShipmentId { get; set; }
    }
}
