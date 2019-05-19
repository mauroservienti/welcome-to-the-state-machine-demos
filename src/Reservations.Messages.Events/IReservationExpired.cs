using System;

namespace Reservations.Messages.Events
{
    public interface IReservationExpired
    {
        Guid ReservationId { get; }
    }
}
