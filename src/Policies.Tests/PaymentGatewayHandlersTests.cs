using Finance.PaymentGateway.Handlers;
using Finance.PaymentGateway.Messages;
using NServiceBus.Testing;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Policies.Tests
{
    public class PaymentGatewayHandlersTests
    {
        [Fact]
        public async Task Handle_AuthorizeCard_RepliesWithCardAuthorizedResponse()
        {
            var reservationId = Guid.NewGuid();
            var handler = new AuthorizeCardHandler();
            var context = new TestableMessageHandlerContext();

            await handler.Handle(new AuthorizeCard
            {
                ReservationId = reservationId,
                PaymentMethodId = 1
            }, context);

            var reply = context.RepliedMessages
                .Select(m => m.Message)
                .OfType<CardAuthorizedResponse>()
                .SingleOrDefault();

            Assert.NotNull(reply);
            Assert.Equal(reservationId, reply.ReservationId);
            Assert.NotEqual(Guid.Empty, reply.AuthorizationId);
        }

        [Fact]
        public async Task Handle_ChargeCard_RepliesWithCardChargedResponse()
        {
            var reservationId = Guid.NewGuid();
            var handler = new ChargeCardHandler();
            var context = new TestableMessageHandlerContext();

            await handler.Handle(new ChargeCard
            {
                ReservationId = reservationId,
                AuthorizationId = Guid.NewGuid()
            }, context);

            var reply = context.RepliedMessages
                .Select(m => m.Message)
                .OfType<CardChargedResponse>()
                .SingleOrDefault();

            Assert.NotNull(reply);
            Assert.Equal(reservationId, reply.ReservationId);
        }
    }
}
