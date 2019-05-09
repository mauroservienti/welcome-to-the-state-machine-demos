using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.PaymentGateway.Messages
{
    public class AuthorizeCardResponse
    {
        public bool Succeeded { get; set; }
        public Guid OrderId { get; set; }
    }
}
