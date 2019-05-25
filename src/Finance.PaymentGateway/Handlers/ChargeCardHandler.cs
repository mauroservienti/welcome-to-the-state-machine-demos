using Finance.PaymentGateway.Messages;
using NServiceBus;
using System;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace Finance.PaymentGateway.Handlers
{
    class ChargeCardHandler : IHandleMessages<ChargeCard>
    {
        public async Task Handle(ChargeCard message, IMessageHandlerContext context)
        {
            Console.WriteLine("Waiting 5\" before replying...", Color.Yellow);

            await Task.Delay(5000);

            await context.Reply(new CardChargedResponse()
            {
                ReservationId = message.ReservationId
            });

            Console.WriteLine($"Payment for reservation '{message.ReservationId}' succeeded.", Color.Green);
        }
    }
}
