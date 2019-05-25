using Finance.PaymentGateway.Messages;
using NServiceBus;
using System;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace Finance.PaymentGateway.Handlers
{
    class AuthorizeCardHandler : IHandleMessages<AuthorizeCard>
    {
        public async Task Handle(AuthorizeCard message, IMessageHandlerContext context)
        {
            /*
             * Use PaymentMethodId to retrieve credit card
             * information from the Vault and process the
             * authorization request
             */
            Console.WriteLine($"Attempt to authorize card with Id '{message.PaymentMethodId}' for reservation '{message.ReservationId}'", Color.Green);
            Console.WriteLine("Waiting 5\" before replying...", Color.Yellow);

            await Task.Delay(5000);

            await context.Reply(new CardAuthorizedResponse()
            {
                ReservationId = message.ReservationId,
                AuthorizationId = Guid.NewGuid()
            });

            Console.WriteLine($"Payment for reservation '{message.ReservationId}' authorized.", Color.Green);
        }
    }
}
