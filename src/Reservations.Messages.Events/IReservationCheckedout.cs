using System;

namespace Reservations.Messages.Events
{
    public interface IReservationCheckedout
    {
        Guid ReservationId { get; }
        int[] Tickets { get; }
    }
}
