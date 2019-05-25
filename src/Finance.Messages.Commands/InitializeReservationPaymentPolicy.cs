using System;

namespace Finance.Messages.Commands
{
    public class InitializeReservationPaymentPolicy
    {
        public Guid ReservationId { get; set; }
        public int PaymentMethodId { get; set; }
    }
}
