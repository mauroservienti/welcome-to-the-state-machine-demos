using Finance.Data;
using Finance.Data.Models;
using Finance.Messages.Commands;
using NServiceBus;
using System.Threading.Tasks;

namespace Finance.Service.Handlers
{
    class StoreReservedTicketHandler : IHandleMessages<StoreReservedTicket>
    {
        public async Task Handle(StoreReservedTicket message, IMessageHandlerContext context)
        {
            using (var db = FinanceContext.Create())
            {
                db.ReservedTickets.Add(new ReservedTicket()
                {
                    ReservationId = message.ReservationId,
                    TicketId = message.TicketId
                });

                await db.SaveChangesAsync();
            }
        }
    }
}
