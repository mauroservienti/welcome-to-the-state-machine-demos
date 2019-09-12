using System;

namespace Shipping.Messages.Events
{
    public interface IOrderShipped
    {
        Guid ReservationId { get; }
        Guid OrderId { get; }
        Guid ShipmentId { get; }
    }
}
