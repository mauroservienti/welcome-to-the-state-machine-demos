using NServiceBus;
using Reservations.Messages.Events;
using Sales.Data;
using Sales.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sales.Service.Handlers
{
    class ReservationCheckedoutHandler : IHandleMessages<IReservationCheckedout>
    {
        public async Task Handle(IReservationCheckedout message, IMessageHandlerContext context)
        {
            using (var db = SalesContext.Create())
            {
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    ReservationId = message.ReservationId
                };
                order.OrderedTickets = message.Tickets
                    .GroupBy(t => t)
                    .Select(g => new OrderedTicket()
                    {
                        OrderId = order.Id,
                        TicketId = g.Key,
                        Quantity = g.Count(),
                    }).ToList();

                /*
                 * Demo utilizes LearningTransport and SQL, with no
                 * Outbox configured to simplify the F5 experience.
                 * This, however, means that the Publish operation
                 * and the below database transaction are not atomic.
                 * 
                 * There isn't really a chance for the LearningTransport
                 * to fail, but in production Outbox should be used when
                 * using transports with no support for transactions.
                 */
                await context.Publish(new Messages.OrderCreated()
                {
                    OrderId = order.Id,
                    ReservationId = message.ReservationId
                });

                db.Orders.Add(order);
                await db.SaveChangesAsync();
            }
        }
    }
}
