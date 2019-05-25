using System;

namespace Finance.PaymentGateway.Messages
{
    public class ChargeCard
    {
        public Guid ReservationId { get; set; }
        public Guid AuthorizationId { get; set; }
    }
}
