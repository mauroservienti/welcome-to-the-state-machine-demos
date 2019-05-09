using System;

namespace Finance.Service.Messages
{
    class InitiatePaymentProcessing
    {
        public Guid OrderId { get; set; }
    }
}
