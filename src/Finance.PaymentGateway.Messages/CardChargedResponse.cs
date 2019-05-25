using System;

namespace Finance.PaymentGateway.Messages
{
    public class CardChargedResponse
    {
        public Guid ReservationId { get; set; }
    }
}
