using Finance.PaymentGateway.Messages;
using NServiceBus;
using System.Threading.Tasks;

namespace Finance.PaymentGateway.Handlers
{
    class ReleaseCardAuthorizationHandler : IHandleMessages<ReleaseCardAuthorization>
    {
        public Task Handle(ReleaseCardAuthorization message, IMessageHandlerContext context)
        {
            /*
             * contact the credit card provider and release
             * the authorized transaction identified by the
             * incoming message TransactionId.
             */

            return Task.CompletedTask;
        }
    }
}
