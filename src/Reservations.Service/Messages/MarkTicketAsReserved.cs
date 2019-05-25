using System;

namespace Reservations.Service.Messages
{
    class MarkTicketAsReserved
    {
        public int TicketId { get; set; }
        public Guid ReservationId { get;  set; }
    }
}
