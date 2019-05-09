using System;

namespace Ticketing.Messages.Events
{
    public interface IOrderCheckedOut
    {
        Guid CustomerId { get; }
        Guid OrderId { get; }
    }
}