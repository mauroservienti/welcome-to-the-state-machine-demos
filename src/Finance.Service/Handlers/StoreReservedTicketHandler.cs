using Finance.Data;
using Finance.Data.Models;
using Finance.Messages.Commands;
using NServiceBus;
using System;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace Finance.Service.Handlers
{
    class StoreReservedTicketHandler : IHandleMessages<StoreReservedTicket>
    {
        readonly Func<FinanceContext> contextFactory;

        public StoreReservedTicketHandler() : this(() => new FinanceContext())
        {
        }

        internal StoreReservedTicketHandler(Func<FinanceContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public async Task Handle(StoreReservedTicket message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Adding ticket '{message.TicketId}' to reservation '{message.ReservationId}'.", Color.Green);

            await using var db = contextFactory();
            db.ReservedTickets.Add(new ReservedTicket() { ReservationId = message.ReservationId, TicketId = message.TicketId });
            await db.SaveChangesAsync(context.CancellationToken);


            Console.WriteLine($"Ticket added.", Color.Green);
        }
    }
}
