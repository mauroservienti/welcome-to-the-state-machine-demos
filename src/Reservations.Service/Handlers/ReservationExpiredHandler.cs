using Microsoft.EntityFrameworkCore;
using NServiceBus;
using Reservations.Data;
using Reservations.Messages.Events;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace Reservations.Service.Handlers
{
    class ReservationExpiredHandler : IHandleMessages<IReservationExpired>
    {
        public async Task Handle(IReservationExpired message, IMessageHandlerContext context)
        {
            Console.WriteLine($"IReservationExpired received for reservation'{message.ReservationId}'.", Color.Green);
            
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

            Console.WriteLine($"All reserved tickets deleted.", Color.Green);
        }
    }
}
