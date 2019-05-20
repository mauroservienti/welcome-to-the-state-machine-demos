using Finance.Messages.Events;
using System;

namespace Finance.Service.Messages
{
    class PaymentAuthorized : IPaymentAuthorized
    {
        public Guid ReservationId { get; set; }
    }
}
