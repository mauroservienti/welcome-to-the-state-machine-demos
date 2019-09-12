using System;

namespace Reservations.Messages.Events
{
    public interface IOrderCreated
    {
        Guid ReservationId { get; }
        Guid OrderId { get; }
    }
}
