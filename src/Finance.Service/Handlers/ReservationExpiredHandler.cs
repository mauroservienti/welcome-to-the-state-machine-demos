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
                var save = false;
                //very inefficient way of deleting stuff.
                var reservations = await db.ReservedTickets
                    .Where(r => r.ReservationId == message.ReservationId)
                    .ToListAsync();
                if (reservations.Any())
                {
                    db.RemoveRange(reservations);
                    save = true;
                }

                var paymentMethods = await db.ReservationsPaymentMethod
                    .Where(r => r.Id == message.ReservationId)
                    .ToListAsync();
                if (paymentMethods.Any())
                {
                    db.RemoveRange(paymentMethods);
                    save = true;
                }

                if (save)
                {
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
