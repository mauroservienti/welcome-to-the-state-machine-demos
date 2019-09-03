using Reservations.Messages.Events;
using System;

namespace Reservations.Service.Messages
{
    class OrderCreated : IOrderCreated
    {
        public Guid ReservationId { get; set; }
        public Guid OrderId { get; set; }
    }
}
