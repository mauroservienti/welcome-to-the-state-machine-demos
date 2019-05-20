using System;

namespace Reservations.Messages.Commands
{
    public class CheckoutReservation
    {
        public Guid ReservationId { get; set; }
    }
}
