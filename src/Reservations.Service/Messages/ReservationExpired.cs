using Reservations.Messages.Events;
using System;

namespace Reservations.Service.Messages
{
    class ReservationExpired : IReservationExpired
    {
        public Guid ReservationId { get; set; }
    }
}
