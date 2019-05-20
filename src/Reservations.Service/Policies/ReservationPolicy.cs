using NServiceBus;
using Reservations.Messages.Commands;
using Reservations.Service.Messages;
using System;
using System.Threading.Tasks;

namespace Reservations.Service.Policies
{
    class ReservationPolicy : Saga<ReservationPolicy.State>,
        IAmStartedByMessages<ReserveTicket>,
        IHandleMessages<CheckoutReservation>,
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
            mapper.ConfigureMapping<CheckoutReservation>(m => m.ReservationId).ToSaga(s => s.ReservationId);
        }

        public async Task Handle(ReserveTicket message, IMessageHandlerContext context)
        {
            Data.ReservationId = message.ReservationId;

            await context.SendLocal(new MarkTicketAsReserved()
            {
                ReservationId = message.ReservationId,
                TicketId = message.TicketId
            });

            if (!Data.CountdownStarted)
            {
                await RequestTimeout<TicketsReservationTimeout>(context, TimeSpan.FromMinutes(5));
                Data.CountdownStarted = true;
            }
        }

        public async Task Timeout(TicketsReservationTimeout state, IMessageHandlerContext context)
        {
            await context.Publish(new ReservationExpired()
            {
                ReservationId = Data.ReservationId
            });
            MarkAsComplete();
        }

        public async Task Handle(CheckoutReservation message, IMessageHandlerContext context)
        {
            /*
             * The demo ignores that a reservation could be checked out
             * after the timeout was expired. In such scenario there won't
             * be any reservation to checkout and the incoming message is 
             * simply "lost". Reservations Service should have a
             * IHandleSagaNotFound implementation to catch this scenario
             * 
             * https://docs.particular.net/nservicebus/sagas/saga-not-found
             */
            await context.Publish(new ReservationCheckedout()
            {
                ReservationId = Data.ReservationId
            });
            MarkAsComplete();
        }
    }

    class TicketsReservationTimeout
    {

    }
}
