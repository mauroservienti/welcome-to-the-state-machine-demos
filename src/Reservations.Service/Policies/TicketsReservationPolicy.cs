using NServiceBus;
using Reservations.Messages.Commands;
using Reservations.Service.Messages;
using System;
using System.Threading.Tasks;

namespace Reservations.Service.Policies
{
    class TicketsReservationPolicy : Saga<TicketsReservationPolicy.State>,
        IAmStartedByMessages<ReserveTicket>,
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
                await RequestTimeout<TicketsReservationTimeout>(context, TimeSpan.FromSeconds(20));
                Data.CountdownStarted = true;
            }
        }

        public async Task Timeout(TicketsReservationTimeout state, IMessageHandlerContext context)
        {
            await context.SendLocal(new ReleaseReservedTickets()
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
