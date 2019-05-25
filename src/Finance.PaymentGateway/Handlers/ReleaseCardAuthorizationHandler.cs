using Finance.PaymentGateway.Messages;
using NServiceBus;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace Finance.PaymentGateway.Handlers
{
    class ReleaseCardAuthorizationHandler : IHandleMessages<ReleaseCardAuthorization>
    {
        public Task Handle(ReleaseCardAuthorization message, IMessageHandlerContext context)
        {
            /*
             * contact the credit card provider and release
             * the authorized transaction identified by the
             * incoming message AuthorizationId.
             */

            Console.WriteLine($"Releasing card authorization '{message.AuthorizationId}' for reservation '{message.ReservationId}'", Color.Green);
            return Task.CompletedTask;
        }
    }
}
