using NServiceBus.Testing;
using Reservations.Messages.Commands;
using Reservations.Messages.Events;
using Reservations.Service.Messages;
using Reservations.Service.Policies;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Policies.Tests
{
    public class ReservationPolicyTests
    {
        [Fact]
        public async Task Handle_ReserveTicket_SendsMarkTicketAsReserved()
        {
            var reservationId = Guid.NewGuid();
            var ticketId = 42;

            var saga = new ReservationPolicy { Data = new ReservationPolicy.State() };
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new ReserveTicket { ReservationId = reservationId, TicketId = ticketId }, context);

            var sentMessage = context.SentMessages
                .Select(m => m.Message)
                .OfType<MarkTicketAsReserved>()
                .SingleOrDefault();

            Assert.NotNull(sentMessage);
            Assert.Equal(reservationId, sentMessage.ReservationId);
            Assert.Equal(ticketId, sentMessage.TicketId);
        }

        [Fact]
        public async Task Handle_ReserveTicket_SetsReservationIdOnSagaData()
        {
            var reservationId = Guid.NewGuid();

            var saga = new ReservationPolicy { Data = new ReservationPolicy.State() };
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new ReserveTicket { ReservationId = reservationId, TicketId = 1 }, context);

            Assert.Equal(reservationId, saga.Data.ReservationId);
        }

        [Fact]
        public async Task Handle_ReservationCheckedout_CompletesTheSaga()
        {
            var reservationId = Guid.NewGuid();

            var saga = new ReservationPolicy
            {
                Data = new ReservationPolicy.State { ReservationId = reservationId }
            };
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new TestableReservationCheckedout { ReservationId = reservationId }, context);

            Assert.True(saga.Completed);
        }

        [Fact]
        public async Task Handle_ReservationCheckedout_DoesNotSendOrPublishMessages()
        {
            var reservationId = Guid.NewGuid();

            var saga = new ReservationPolicy
            {
                Data = new ReservationPolicy.State { ReservationId = reservationId }
            };
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new TestableReservationCheckedout { ReservationId = reservationId }, context);

            Assert.Empty(context.SentMessages);
            Assert.Empty(context.PublishedMessages);
        }

        class TestableReservationCheckedout : IReservationCheckedout
        {
            public Guid ReservationId { get; set; }
            public int[] Tickets { get; set; } = [];
        }
    }
}
