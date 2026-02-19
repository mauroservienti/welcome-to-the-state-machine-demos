using Finance.Messages.Events;
using NServiceBus.Testing;
using Reservations.Messages.Events;
using Shipping.Data;
using Shipping.Messages.Commands;
using Shipping.Messages.Events;
using Shipping.Service.Policies;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Policies.Tests
{
    public class ShippingPolicyTests
    {
        static ShippingPolicy CreateSaga(Action<ShippingPolicyState>? configure = null)
        {
            var data = new ShippingPolicyState();
            configure?.Invoke(data);
            return new ShippingPolicy { Data = data };
        }

        [Fact]
        public async Task Handle_InitializeReservationShippingPolicy_SetsDeliveryOption()
        {
            var reservationId = Guid.NewGuid();
            var saga = CreateSaga();
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new InitializeReservationShippingPolicy
            {
                ReservationId = reservationId,
                DeliveryOption = DeliveryOptions.ShipAtHome
            }, context);

            Assert.True(saga.Data.DeliveryOptionDefined);
            Assert.Equal(DeliveryOptions.ShipAtHome, saga.Data.DeliveryOption);
        }

        [Fact]
        public async Task Handle_InitializeReservationShippingPolicy_Alone_DoesNotStartShipment()
        {
            var saga = CreateSaga();
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new InitializeReservationShippingPolicy
            {
                ReservationId = Guid.NewGuid(),
                DeliveryOption = DeliveryOptions.ShipAtHome
            }, context);

            Assert.False(saga.Completed);
            Assert.Empty(context.PublishedMessages);
            Assert.Empty(context.SentMessages);
        }

        [Fact]
        public async Task Handle_OrderCreated_SetsOrderCreatedFlag()
        {
            var reservationId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var saga = CreateSaga();
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new TestableOrderCreated { ReservationId = reservationId, OrderId = orderId }, context);

            Assert.True(saga.Data.OrderCreated);
            Assert.Equal(orderId, saga.Data.OrderId);
            Assert.Equal(reservationId, saga.Data.ReservationId);
        }

        [Fact]
        public async Task Handle_OrderCreated_Alone_DoesNotStartShipment()
        {
            var saga = CreateSaga();
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new TestableOrderCreated { ReservationId = Guid.NewGuid(), OrderId = Guid.NewGuid() }, context);

            Assert.False(saga.Completed);
            Assert.Empty(context.PublishedMessages);
            Assert.Empty(context.SentMessages);
        }

        [Fact]
        public async Task Handle_PaymentSucceeded_SetsPaymentSucceededFlag()
        {
            var saga = CreateSaga();
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new TestablePaymentSucceeded { ReservationId = Guid.NewGuid(), OrderId = Guid.NewGuid() }, context);

            Assert.True(saga.Data.PaymentSucceeded);
        }

        [Fact]
        public async Task Handle_PaymentSucceeded_Alone_DoesNotStartShipment()
        {
            var saga = CreateSaga();
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new TestablePaymentSucceeded { ReservationId = Guid.NewGuid(), OrderId = Guid.NewGuid() }, context);

            Assert.False(saga.Completed);
            Assert.Empty(context.PublishedMessages);
            Assert.Empty(context.SentMessages);
        }

        [Fact]
        public async Task Handle_AllConditionsMet_ShipAtHome_PublishesOrderShippedAndCompletes()
        {
            var reservationId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var saga = CreateSaga(d =>
            {
                d.ReservationId = reservationId;
                d.OrderId = orderId;
                d.OrderCreated = true;
                d.PaymentSucceeded = true;
                d.DeliveryOptionDefined = true;
                d.DeliveryOption = DeliveryOptions.ShipAtHome;
            });
            var context = new TestableMessageHandlerContext();

            // Trigger the final condition check via any of the three handlers
            await saga.Handle(new TestablePaymentSucceeded { ReservationId = reservationId, OrderId = orderId }, context);

            Assert.True(saga.Completed);

            var publishedEvent = context.PublishedMessages
                .Select(m => m.Message)
                .OfType<IOrderShipped>()
                .SingleOrDefault();

            Assert.NotNull(publishedEvent);
            Assert.Equal(orderId, publishedEvent.OrderId);
            Assert.Equal(reservationId, publishedEvent.ReservationId);
            Assert.NotEqual(Guid.Empty, publishedEvent.ShipmentId);
        }

        [Fact]
        public async Task Handle_AllConditionsMet_CollectAtTheVenue_SendsStoreReservationForVenueDeliveryAndCompletes()
        {
            var reservationId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var saga = CreateSaga(d =>
            {
                d.ReservationId = reservationId;
                d.OrderId = orderId;
                d.OrderCreated = true;
                d.PaymentSucceeded = true;
                d.DeliveryOptionDefined = true;
                d.DeliveryOption = DeliveryOptions.CollectAtTheVenue;
            });
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new TestablePaymentSucceeded { ReservationId = reservationId, OrderId = orderId }, context);

            Assert.True(saga.Completed);

            var sentMessage = context.SentMessages
                .Select(m => m.Message)
                .OfType<StoreReservationForVenueDelivery>()
                .SingleOrDefault();

            Assert.NotNull(sentMessage);
            Assert.Equal(orderId, sentMessage.OrderId);
            Assert.Equal(reservationId, sentMessage.ReservationId);
        }

        [Fact]
        public async Task Handle_AllConditionsMet_ShipAtHome_DoesNotSendVenueDelivery()
        {
            var reservationId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var saga = CreateSaga(d =>
            {
                d.ReservationId = reservationId;
                d.OrderId = orderId;
                d.OrderCreated = true;
                d.PaymentSucceeded = true;
                d.DeliveryOptionDefined = true;
                d.DeliveryOption = DeliveryOptions.ShipAtHome;
            });
            var context = new TestableMessageHandlerContext();

            await saga.Handle(new TestablePaymentSucceeded { ReservationId = reservationId, OrderId = orderId }, context);

            Assert.Empty(context.SentMessages);
        }

        class TestableOrderCreated : IOrderCreated
        {
            public Guid ReservationId { get; set; }
            public Guid OrderId { get; set; }
        }

        class TestablePaymentSucceeded : IPaymentSucceeded
        {
            public Guid ReservationId { get; set; }
            public Guid OrderId { get; set; }
        }
    }
}
