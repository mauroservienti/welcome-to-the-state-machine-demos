using System;
using System.Collections.Generic;

namespace Reservations.Data.Models
{
    public class Reservation
    {
        public Guid Id { get; set; }
        public List<ReservedTicket> ReservedTickets { get; set; } = new List<ReservedTicket>();
    }

    public class ReservedTicket
    {
        public int Id { get; set; }
        public Guid ReservationId { get; set; }
        public int TicketId { get; set; }
    }
}
