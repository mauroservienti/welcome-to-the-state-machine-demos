using Finance.Data;
using Finance.Data.Models;
using Finance.Messages.Commands;
using NServiceBus;
using System.Threading.Tasks;

namespace Finance.Service.Handlers
{
    class TrackReservedTicketHandler : IHandleMessages<TrackReservedTicket>
    {
        public async Task Handle(TrackReservedTicket message, IMessageHandlerContext context)
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
