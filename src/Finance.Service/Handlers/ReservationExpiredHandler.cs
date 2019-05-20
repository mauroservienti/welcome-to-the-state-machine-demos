using Finance.Data;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using Reservations.Messages.Events;
using System.Linq;
using System.Threading.Tasks;

namespace Finance.Service.Handlers
{
    class ReservationExpiredHandler : IHandleMessages<IReservationExpired>
    {
        public async Task Handle(IReservationExpired message, IMessageHandlerContext context)
        {
            using (var db = FinanceContext.Create())
            {
                //very inefficient way of deleting stuff.
                var reservations = await db.ReservedTickets
                    .Where(r => r.ReservationId == message.ReservationId)
                    .ToListAsync();
                if (reservations.Any())
                {
                    db.RemoveRange(reservations);
                }

                await db.SaveChangesAsync();
            }
        }
    }
}
