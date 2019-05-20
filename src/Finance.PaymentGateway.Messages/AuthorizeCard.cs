using System;

namespace Finance.PaymentGateway.Messages
{
    public class AuthorizeCard
    {
        public Guid ReservationId { get; set; }
        public int PaymentMethodId { get; set; }
    }
}
