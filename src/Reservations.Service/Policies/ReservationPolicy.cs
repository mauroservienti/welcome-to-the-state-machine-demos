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
        IHandleMessages<IReservationCheckedout>,
        IHandleTimeouts<TicketsReservationTimeout>
    {
        public class State : ContainSagaData
        {
            public Guid ReservationId { get; set; }
            public bool CountdownStarted { get; set; }
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<State> mapper)
        {
            mapper.ConfigureMapping<ReserveTicket>(m => m.ReservationId).ToSaga(s => s.ReservationId);
            mapper.ConfigureMapping<IReservationCheckedout>(m => m.ReservationId).ToSaga(s => s.ReservationId);
        }

        public async Task Handle(ReserveTicket message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Addgin ticket '{message.TicketId}' to reservation '{message.ReservationId}'.", Color.Green);

            Data.ReservationId = message.ReservationId;

            await context.SendLocal(new MarkTicketAsReserved()
            {
                ReservationId = message.ReservationId,
                TicketId = message.TicketId
            });

            Console.WriteLine($"MarkTicketAsReserved request sent.", Color.Green);
            
            if (!Data.CountdownStarted)
            {
                await RequestTimeout<TicketsReservationTimeout>(context, TimeSpan.FromMinutes(5));
                Data.CountdownStarted = true;
                
                Console.WriteLine($"TicketsReservationTimeout started for reservation '{message.ReservationId}'.", Color.Green);
            }
        }

        public async Task Timeout(TicketsReservationTimeout state, IMessageHandlerContext context)
        {
            Console.WriteLine($"TicketsReservationTimeout expired.", Color.Green);
            await context.Publish(new ReservationExpired()
            {
                ReservationId = Data.ReservationId
            });
            MarkAsComplete();

            Console.WriteLine($"I'm done. ReservationExpired event published.", Color.Green);
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

    class TicketsReservationTimeout
    {

    }
}
