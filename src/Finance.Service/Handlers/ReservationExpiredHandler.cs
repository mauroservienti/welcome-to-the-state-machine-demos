using Finance.Data;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using Reservations.Messages.Events;
using System.Linq;
using System.Threading.Tasks;
using Console = Colorful.Console;
using System.Drawing;

namespace Finance.Service.Handlers
{
    class ReservationExpiredHandler : IHandleMessages<IReservationExpired>
    {
        public async Task Handle(IReservationExpired message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Reservation '{message.ReservationId}' expired, releasing reserved tickets.", Color.Green);

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

            Console.WriteLine("Tickets released.", Color.Green);
        }
    }
}
