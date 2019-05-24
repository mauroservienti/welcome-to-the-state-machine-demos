using System;

namespace Sales.Messages.Events
{
    public interface IOrderCreated
    {
        Guid ReservationId { get; }
        Guid OrderId { get; }
    }
}
