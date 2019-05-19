using System;

namespace Finance.Messages.Commands
{
    public class TrackReservedTicket
    {
        public int TicketId { get; set; }
        public Guid ReservationId { get; set; }
    }
}
