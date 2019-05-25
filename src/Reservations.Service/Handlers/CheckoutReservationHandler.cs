using Microsoft.EntityFrameworkCore;
using NServiceBus;
using Reservations.Data;
using Reservations.Messages.Commands;
using Reservations.Service.Messages;
using System.Linq;
using System.Threading.Tasks;

namespace Reservations.Service.Handlers
{
    class CheckoutReservationHandler : IHandleMessages<CheckoutReservation>
    {
        public async Task Handle(CheckoutReservation message, IMessageHandlerContext context)
        {
            using (var db = ReservationsContext.Create())
            {
                var reservation = await db.Reservations
                    .Where(r => r.Id == message.ReservationId)
                    .Include(r => r.ReservedTickets)
                    .SingleOrDefaultAsync();

                /*
                 * The demo ignores that a reservation could be checked out
                 * after the timeout is expired. In such scenario there won't
                 * be any reservation to checkout and the incoming message is 
                 * simply "lost", or leads to a failure.
                 */
                await context.Publish(new ReservationCheckedout()
                {
                    ReservationId = message.ReservationId,
                    Tickets = reservation.ReservedTickets
                        .Select(rt => rt.TicketId)
                        .ToArray()
                });

                db.Reservations.Remove(reservation);
                await db.SaveChangesAsync();
            }
        }
    }
}
