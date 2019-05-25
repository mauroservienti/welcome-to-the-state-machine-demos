using System;

namespace Finance.Messages.Events
{
    public interface IPaymentSucceeded
    {
        Guid ReservationId { get; }
        Guid OrderId { get; }
    }
}
