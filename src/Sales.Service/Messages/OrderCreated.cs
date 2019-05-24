using Sales.Messages.Events;
using System;

namespace Sales.Service.Messages
{
    class OrderCreated : IOrderCreated
    {
        public Guid ReservationId { get; set; }
        public Guid OrderId { get; set; }
    }
}
