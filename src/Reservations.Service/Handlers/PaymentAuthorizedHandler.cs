using Finance.Messages.Events;
using NServiceBus;
using System.Threading.Tasks;

namespace Reservations.Service.Handlers
{
    class PaymentAuthorizedHandler : IHandleMessages<IPaymentAuthorized>
    {
        public Task Handle(IPaymentAuthorized message, IMessageHandlerContext context)
        {
            //load reservation
            //convert to order (or better name)
            //raise Tickets Assigned or better yet Order Created
            //might not even be a Reservations responsibility and something like Sales should be introduced...

            return Task.CompletedTask;
        }
    }
}
