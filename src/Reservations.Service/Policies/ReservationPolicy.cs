using NServiceBus;
using Reservations.Messages.Commands;
using Reservations.Messages.Events;
using Reservations.Service.Messages;
using System;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace Reservations.Service.Policies
{
    class ReservationPolicy : Saga<ReservationPolicy.State>,
        IAmStartedByMessages<ReserveTicket>,
        IHandleMessages<IReservationCheckedout>
    {
        public class State : ContainSagaData
        {
            public Guid ReservationId { get; set; }
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<State> mapper)
        {
            mapper.MapSaga(saga => saga.ReservationId)
                .ToMessage<ReserveTicket>(m => m.ReservationId)
                .ToMessage<IReservationCheckedout>(m => m.ReservationId);
        }

        public async Task Handle(ReserveTicket message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Adding ticket '{message.TicketId}' to reservation '{message.ReservationId}'.", Color.Green);

            Data.ReservationId = message.ReservationId;

            /*
             * Tickets are reserved indefinitely. Usually tickets booking websites
             * Allow people to reserve a ticket, or keep it in the shopping cart for
             * a fixed amount of time, e.g. 10 minutes. After 10 minutes if tickets
             * are not purchased they are automatically released back to the pool
             * of available tickets. This can be easily achieved using a timeout in
             * this reservation saga. The timeout should be kicked of once the first
             * time a ticket is reserved. If the timeout expires and the reservation
             * is still existing all reserved tickets can be released.
             */
            await context.SendLocal(new MarkTicketAsReserved()
            {
                ReservationId = message.ReservationId,
                TicketId = message.TicketId
            });

            Console.WriteLine($"MarkTicketAsReserved request sent.", Color.Green);
        }

        public Task Handle(IReservationCheckedout message, IMessageHandlerContext context)
        {
            /*
             * We're done.
             * 
             * We can debate if this should complete when the payment is authorized
             * or, as we are doing, a little earlier. By using the IReservationCheckedout
             * we could fall into the following scenario:
             * - the reservation is checked out
             * - the card authorization fails
             * - the reservation is gone.
             * 
             * Poor experience. On the other end by waiting for the card authorization
             * we're locking tickets for more time. It's a business decision, not a 
             * technical one.
             */

            Console.WriteLine($"IReservationCheckedout, I'm done.", Color.Green);
            MarkAsComplete();
            return Task.CompletedTask;
        }
    }
}
