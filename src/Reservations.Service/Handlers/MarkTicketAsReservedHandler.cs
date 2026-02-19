using Microsoft.EntityFrameworkCore;
using NServiceBus;
using Reservations.Data;
using Reservations.Data.Models;
using Reservations.Service.Messages;
using System;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace Reservations.Service.Handlers
{
    class MarkTicketAsReservedHandler : IHandleMessages<MarkTicketAsReserved>
    {
        readonly Func<ReservationsContext> contextFactory;

        public MarkTicketAsReservedHandler() : this(() => new ReservationsContext())
        {
        }

        internal MarkTicketAsReservedHandler(Func<ReservationsContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public async Task Handle(MarkTicketAsReserved message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Going to mark ticket '{message.TicketId}' as reserved.", Color.Green);

            await using var db = contextFactory();
            var reservation = await db.Reservations
                .Where(r=>r.Id==message.ReservationId)
                .Include(r=>r.ReservedTickets)
                .SingleOrDefaultAsync(context.CancellationToken);

            if (reservation == null)
            {
                reservation = new Reservation()
                {
                    Id = message.ReservationId
                };
                db.Reservations.Add(reservation);
            }
            else if (reservation.ReservedTickets.Any(rt => rt.TicketId == message.TicketId))
            {
                return;
            }

            reservation.ReservedTickets.Add(new ReservedTicket()
            {
                ReservationId = message.ReservationId,
                TicketId = message.TicketId
            });

            await db.SaveChangesAsync(context.CancellationToken);

            Console.WriteLine($"Ticket '{message.TicketId}' reserved to reservation '{message.ReservationId}'.", Color.Green);
        }
    }
}
