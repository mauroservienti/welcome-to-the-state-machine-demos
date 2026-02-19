using Finance.Data;
using Finance.Data.Models;
using Finance.Messages.Commands;
using Microsoft.EntityFrameworkCore;
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
            var alreadyStored = await db.ReservedTickets.AnyAsync(
                rt => rt.ReservationId == message.ReservationId && rt.TicketId == message.TicketId,
                context.CancellationToken);
            if (alreadyStored)
            {
                return;
            }

            db.ReservedTickets.Add(new ReservedTicket() { ReservationId = message.ReservationId, TicketId = message.TicketId });
            await db.SaveChangesAsync(context.CancellationToken);


            Console.WriteLine($"Ticket added.", Color.Green);
        }
    }
}
