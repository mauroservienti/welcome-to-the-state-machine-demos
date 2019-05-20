using Finance.PaymentGateway.Messages;
using NServiceBus;
using System;
using System.Threading.Tasks;

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
            Console.WriteLine("Waiting 5\" before replying...");

            await Task.Delay(5000);

            await context.Reply(new CardAuthorizedResponse()
            {
                ReservationId = message.ReservationId
            });

            Console.WriteLine($"Payment for reservation '{message.ReservationId}' authorized.");
        }
    }
}
