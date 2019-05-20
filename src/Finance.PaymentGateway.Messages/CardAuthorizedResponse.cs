using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.PaymentGateway.Messages
{
    public class CardAuthorizedResponse
    {
        public Guid ReservationId { get; set; }
    }
}
