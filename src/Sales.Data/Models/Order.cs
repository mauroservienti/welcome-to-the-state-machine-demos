using System;
using System.Collections.Generic;

namespace Sales.Data.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid ReservationId { get; set; }
        public List<OrderedTicket> OrderedTickets { get; set; } = new List<OrderedTicket>();
    }

    public class OrderedTicket
    {
        public int Id { get; set; }
        public Guid OrderId { get; set; }
        public int TicketId { get; set; }
        public int Quantity { get; set; }
    }
}
