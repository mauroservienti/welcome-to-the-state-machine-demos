using Finance.Messages.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Service.Messages
{
    class PaymentAuthorized : IPaymentAuthorized
    {
        public Guid ReservationId { get; set; }
    }
}
