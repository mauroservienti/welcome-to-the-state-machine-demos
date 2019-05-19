using System;

namespace Finance.Service.Messages
{
    class InitiatePaymentProcessing
    {
        public Guid ReservationId { get; set; }
    }
}
