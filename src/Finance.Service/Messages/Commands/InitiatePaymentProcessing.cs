using System;

namespace Finance.Service.Messages.Commands
{
    class InitiatePaymentProcessing
    {
        public Guid ReservationId { get; set; }
    }
}
