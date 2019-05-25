using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.PaymentGateway.Messages
{
    public class ReleaseCardAuthorization
    {
        public Guid ReservationId { get; set; }
        public Guid TransactionId { get; set; }
    }
}
