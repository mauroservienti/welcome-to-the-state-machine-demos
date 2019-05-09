using System;

namespace Finance.PaymentGateway.Messages
{
    public class AuthorizeCard
    {
        public Guid OrderId { get; set; }
    }
}
