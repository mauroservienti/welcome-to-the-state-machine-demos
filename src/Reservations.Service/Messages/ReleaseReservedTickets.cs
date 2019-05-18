using System;

namespace Reservations.Service.Messages
{
    class ReleaseReservedTickets
    {
        public Guid ReservationId { get; set; }
    }
}
