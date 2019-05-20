using Reservations.Messages.Events;
using System;

namespace Reservations.Service.Messages
{
    class ReservationCheckedout : IReservationCheckedout
    {
        public Guid ReservationId { get; set; }
    }
}
