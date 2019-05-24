using Reservations.Messages.Events;
using System;
using System.Collections.Generic;

namespace Reservations.Service.Messages
{
    class ReservationCheckedout : IReservationCheckedout
    {
        public Guid ReservationId { get; set; }
        public int[] Tickets { get; set; }
    }
}
