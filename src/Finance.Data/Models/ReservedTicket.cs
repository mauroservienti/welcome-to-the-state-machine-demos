using System;

namespace Finance.Data.Models
{
    public class ReservedTicket
    {
        public int Id { get; set; }
        public Guid ReservationId { get; set; }
        public int TicketId { get; set; }
    }
}
