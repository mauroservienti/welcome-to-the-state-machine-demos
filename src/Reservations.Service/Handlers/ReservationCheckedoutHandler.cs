using NServiceBus;
using Reservations.Messages.Events;
using Reservations.Data;
using Reservations.Data.Models;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Console = Colorful.Console;


namespace Reservations.Service.Handlers
{
    class ReservationCheckedoutHandler : IHandleMessages<IReservationCheckedout>
    {
        public async Task Handle(IReservationCheckedout message, IMessageHandlerContext context)
        {
            /*
             * Creation of an order is not a Reservations responsibility.
             * It's probably much more a Sales responsibility, in which case
             * having this handler here is a clear boundaries violation.
             * The only reason to keep it here is that an Order does nothing
             * in this demo. Its creation completes the payment process and
             * nothing else. It made little sense to increase even more the
             * complexity of the demo, creating a Sales endpoint just for the
             * purpose of hosting this message handler. If Sales had more
             * responsibilities in the demo it would have deserved its own
             * endpoint.
             */
            Console.WriteLine($"Ready to create order for reservation '{message.ReservationId}'.", Color.Green);
            using (var db = ReservationsContext.Create())
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

                Console.WriteLine($"Order '{order.Id}' created, IOrderCreated event published.", Color.Green);
            }
        }
    }
}
