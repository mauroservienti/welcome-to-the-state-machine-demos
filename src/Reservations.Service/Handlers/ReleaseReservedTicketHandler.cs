using Microsoft.EntityFrameworkCore;
using NServiceBus;
using Reservations.Data;
using Reservations.Service.Messages;
using System.Linq;
using System.Threading.Tasks;

namespace Reservations.Service.Handlers
{
    class ReleaseReservedTicketHandler : IHandleMessages<ReleaseReservedTickets>
    {
        public async Task Handle(ReleaseReservedTickets message, IMessageHandlerContext context)
        {
            using (var db = ReservationsContext.Create())
            {
                var reservation = await db.Reservations
                    .Where(r => r.Id == message.ReservationId)
                    .SingleOrDefaultAsync();

                if (reservation != null)
                {
                    db.Reservations.Remove(reservation);
                }

                await db.SaveChangesAsync();
            }
        }
    }
}
