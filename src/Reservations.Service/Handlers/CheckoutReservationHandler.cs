using Microsoft.EntityFrameworkCore;
using NServiceBus;
using Reservations.Data;
using Reservations.Messages.Commands;
using Reservations.Service.Messages;
using System;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace Reservations.Service.Handlers
{
    class CheckoutReservationHandler : IHandleMessages<CheckoutReservation>
    {
        readonly Func<ReservationsContext> contextFactory;

        public CheckoutReservationHandler() : this(() => new ReservationsContext())
        {
        }

        internal CheckoutReservationHandler(Func<ReservationsContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public async Task Handle(CheckoutReservation message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Going to check-out reservation '{message.ReservationId}'.", Color.Green);

            await using var db = contextFactory();
            var reservation = await db.Reservations
                .Where(r => r.Id == message.ReservationId)
                .Include(r => r.ReservedTickets)
                .SingleOrDefaultAsync(context.CancellationToken);

            /*
                 * In case reservations expires the demo ignores that.
                 * A description of reservations expiration can be found
                 * in the ReservationsPolicy class.
                 * A reservation could be checked out after the Reservation
                 * expiration timeout is expired. In such scenario there won't
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
            await db.SaveChangesAsync(context.CancellationToken);

            Console.WriteLine($"ReservationCheckedout event published and reservation removed from db.", Color.Green);
        }
    }
}
