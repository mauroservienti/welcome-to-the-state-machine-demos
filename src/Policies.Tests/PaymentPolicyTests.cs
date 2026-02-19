using Finance.Messages.Commands;
using Finance.Messages.Events;
using Finance.PaymentGateway.Messages;
using Finance.Service.Messages;
using Finance.Service.Policies;
using NServiceBus.Testing;
using Reservations.Messages.Events;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Policies.Tests
{
    public class PaymentPolicyTests
    {
        static PaymentPolicy CreateSaga(Action<PaymentPolicyState>? configure = null)
        {
            var data = new PaymentPolicyState();
            configure?.Invoke(data);
            return new PaymentPolicy { Data = data };
        }

        [Fact]
        public async Task Handle_ReservationCheckedout_SetsReservationCheckedOutFlag()
        {
            var reservationId = Guid.NewGuid();
            var saga = CreateSaga();
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new TestableReservationCheckedout { ReservationId = reservationId }, context);

            Assert.True(saga.Data.ReservationCheckedOut);
            Assert.Equal(reservationId, saga.Data.ReservationId);
        }

        [Fact]
        public async Task Handle_ReservationCheckedout_WithoutPaymentMethod_DoesNotSendInitiatePaymentProcessing()
        {
            var saga = CreateSaga();
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new TestableReservationCheckedout { ReservationId = Guid.NewGuid() }, context);

            Assert.Empty(context.SentMessages);
        }

        [Fact]
        public async Task Handle_InitializeReservationPaymentPolicy_SetsPaymentMethodSetFlag()
        {
            var reservationId = Guid.NewGuid();
            var paymentMethodId = 7;
            var saga = CreateSaga();
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new InitializeReservationPaymentPolicy
            {
                ReservationId = reservationId,
                PaymentMethodId = paymentMethodId
            }, context);

            Assert.True(saga.Data.PaymentMethodSet);
            Assert.Equal(paymentMethodId, saga.Data.PaymentMethodId);
        }

        [Fact]
        public async Task Handle_InitializeReservationPaymentPolicy_WithoutCheckout_DoesNotSendInitiatePaymentProcessing()
        {
            var saga = CreateSaga();
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new InitializeReservationPaymentPolicy
            {
                ReservationId = Guid.NewGuid(),
                PaymentMethodId = 1
            }, context);

            Assert.Empty(context.SentMessages);
        }

        [Fact]
        public async Task Handle_BothCheckoutAndPaymentMethod_SendsInitiatePaymentProcessing()
        {
            var reservationId = Guid.NewGuid();
            var saga = CreateSaga(d =>
            {
                d.ReservationId = reservationId;
                d.ReservationCheckedOut = true;
            });
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new InitializeReservationPaymentPolicy
            {
                ReservationId = reservationId,
                PaymentMethodId = 1
            }, context);

            var sentMessage = context.SentMessages
                .Select(m => m.Message)
                .OfType<InitiatePaymentProcessing>()
                .SingleOrDefault();

            Assert.NotNull(sentMessage);
            Assert.Equal(reservationId, sentMessage.ReservationId);
        }

        [Fact]
        public async Task Handle_InitiatePaymentProcessing_SendsAuthorizeCard()
        {
            var reservationId = Guid.NewGuid();
            var paymentMethodId = 5;
            var saga = CreateSaga(d =>
            {
                d.ReservationId = reservationId;
                d.PaymentMethodId = paymentMethodId;
            });
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new InitiatePaymentProcessing { ReservationId = reservationId }, context);

            var sentMessage = context.SentMessages
                .Select(m => m.Message)
                .OfType<AuthorizeCard>()
                .SingleOrDefault();

            Assert.NotNull(sentMessage);
            Assert.Equal(reservationId, sentMessage.ReservationId);
            Assert.Equal(paymentMethodId, sentMessage.PaymentMethodId);
        }

        [Fact]
        public async Task Handle_BothPaymentMethodAndCheckout_SendsInitiatePaymentProcessing()
        {
            var reservationId = Guid.NewGuid();
            var saga = CreateSaga(d =>
            {
                d.ReservationId = reservationId;
                d.PaymentMethodSet = true;
                d.PaymentMethodId = 1;
            });
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new TestableReservationCheckedout { ReservationId = reservationId }, context);

            var sentMessage = context.SentMessages
                .Select(m => m.Message)
                .OfType<InitiatePaymentProcessing>()
                .SingleOrDefault();

            Assert.NotNull(sentMessage);
            Assert.Equal(reservationId, sentMessage.ReservationId);
        }

        [Fact]
        public async Task Handle_CardAuthorizedResponse_SetsCardAuthorizedAndPublishesPaymentAuthorized()
        {
            var reservationId = Guid.NewGuid();
            var authorizationId = Guid.NewGuid();
            var saga = CreateSaga(d => d.ReservationId = reservationId);
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new CardAuthorizedResponse
            {
                ReservationId = reservationId,
                AuthorizationId = authorizationId
            }, context);

            Assert.True(saga.Data.CardAuthorized);
            Assert.Equal(authorizationId, saga.Data.PaymentAuthorizationId);

            var publishedEvent = context.PublishedMessages
                .Select(m => m.Message)
                .OfType<IPaymentAuthorized>()
                .SingleOrDefault();

            Assert.NotNull(publishedEvent);
            Assert.Equal(reservationId, publishedEvent.ReservationId);
        }

        [Fact]
        public async Task Handle_CardAuthorizedResponse_RequestsCardAuthorizationTimeout()
        {
            var saga = CreateSaga(d => d.ReservationId = Guid.NewGuid());
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new CardAuthorizedResponse
            {
                ReservationId = saga.Data.ReservationId,
                AuthorizationId = Guid.NewGuid()
            }, context);

            Assert.Single(context.TimeoutMessages);
            Assert.IsType<CardAuthorizationTimeout>(context.TimeoutMessages[0].Message);
        }

        [Fact]
        public async Task Timeout_CardAuthorizationTimeout_SendsReleaseCardAuthorizationAndCompletes()
        {
            var reservationId = Guid.NewGuid();
            var authorizationId = Guid.NewGuid();
            var saga = CreateSaga(d =>
            {
                d.ReservationId = reservationId;
                d.PaymentAuthorizationId = authorizationId;
            });
            var context = new TestableMessageHandlerContext();

            await saga.Timeout(new CardAuthorizationTimeout(), context);

            Assert.True(saga.Completed);

            var sentMessage = context.SentMessages
                .Select(m => m.Message)
                .OfType<ReleaseCardAuthorization>()
                .SingleOrDefault();

            Assert.NotNull(sentMessage);
            Assert.Equal(reservationId, sentMessage.ReservationId);
            Assert.Equal(authorizationId, sentMessage.AuthorizationId);
        }

        [Fact]
        public async Task Handle_OrderCreated_SendsChargeCard()
        {
            var reservationId = Guid.NewGuid();
            var authorizationId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var saga = CreateSaga(d =>
            {
                d.ReservationId = reservationId;
                d.PaymentAuthorizationId = authorizationId;
            });
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new TestableOrderCreated { ReservationId = reservationId, OrderId = orderId }, context);

            Assert.Equal(orderId, saga.Data.OrderId);

            var sentMessage = context.SentMessages
                .Select(m => m.Message)
                .OfType<ChargeCard>()
                .SingleOrDefault();

            Assert.NotNull(sentMessage);
            Assert.Equal(reservationId, sentMessage.ReservationId);
            Assert.Equal(authorizationId, sentMessage.AuthorizationId);
        }

        [Fact]
        public async Task Handle_CardChargedResponse_PublishesPaymentSucceededAndCompletes()
        {
            var reservationId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var saga = CreateSaga(d =>
            {
                d.ReservationId = reservationId;
                d.OrderId = orderId;
            });
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new CardChargedResponse { ReservationId = reservationId }, context);

            Assert.True(saga.Completed);

            var publishedEvent = context.PublishedMessages
                .Select(m => m.Message)
                .OfType<IPaymentSucceeded>()
                .SingleOrDefault();

            Assert.NotNull(publishedEvent);
            Assert.Equal(reservationId, publishedEvent.ReservationId);
            Assert.Equal(orderId, publishedEvent.OrderId);
        }

        class TestableReservationCheckedout : IReservationCheckedout
        {
            public Guid ReservationId { get; set; }
            public int[] Tickets { get; set; } = [];
        }

        class TestableOrderCreated : IOrderCreated
        {
            public Guid ReservationId { get; set; }
            public Guid OrderId { get; set; }
        }
    }
}
