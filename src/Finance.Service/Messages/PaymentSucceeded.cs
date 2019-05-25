using Finance.Messages.Events;
using System;

namespace Finance.Service.Messages
{
    class PaymentSucceeded : IPaymentSucceeded
    {
        public Guid ReservationId { get; set; }
        public Guid OrderId { get; set; }
    }
}
