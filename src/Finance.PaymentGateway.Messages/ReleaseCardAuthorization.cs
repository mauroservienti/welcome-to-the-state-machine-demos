using System;

namespace Finance.PaymentGateway.Messages
{
    public class ReleaseCardAuthorization
    {
        public Guid ReservationId { get; set; }
        public Guid AuthorizationId { get; set; }
    }
}
