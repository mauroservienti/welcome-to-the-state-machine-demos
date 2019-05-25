using Microsoft.EntityFrameworkCore;
using NServiceBus;
using Reservations.Data;
using Reservations.Data.Models;
using Reservations.Service.Messages;
using System.Linq;
using System.Threading.Tasks;

namespace Reservations.Service.Handlers
{
    class MarkTicketAsReservedHandler : IHandleMessages<MarkTicketAsReserved>
    {
        public async Task Handle(MarkTicketAsReserved message, IMessageHandlerContext context)
        {
            using (var db = ReservationsContext.Create())
            {
                var reservation = await db.Reservations
                    .Where(r=>r.Id==message.ReservationId)
                    .Include(r=>r.ReservedTickets)
                    .SingleOrDefaultAsync();

                if (reservation == null)
                {
                    reservation = new Reservation()
                    {
                        Id = message.ReservationId
                    };
                    db.Reservations.Add(reservation);
                }

                reservation.ReservedTickets.Add(new ReservedTicket()
                {
                    ReservationId = message.ReservationId,
                    TicketId = message.TicketId
                });

                await db.SaveChangesAsync();
            }
        }
    }
}
