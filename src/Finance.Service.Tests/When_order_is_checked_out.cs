using FakeItEasy;
using Finance.Service.Messages;
using Finance.Service.Policies;
using NServiceBus.Testing;
using System;
using System.Linq;
using System.Threading.Tasks;
using Ticketing.Messages.Events;
using Xunit;

namespace Finance.Service.Tests
{
    public class When_order_is_checked_out
    {
        [Fact]
        public async Task Payment_policy_is_expected_state()
        {
            // arrange
            var saga = new PaymentPolicy
            {
                Data = new PaymentPolicyState()
            };

            var orderCheckedOut = A.Fake<IOrderCheckedOut>();

            A.CallTo(() => orderCheckedOut.CustomerId).Returns(Guid.NewGuid());
            A.CallTo(() => orderCheckedOut.OrderId).Returns(Guid.NewGuid());

            // act
            await saga.Handle(orderCheckedOut, new TestableMessageHandlerContext());

            // assert
            Assert.Equal(orderCheckedOut.OrderId, saga.Data.OrderId);
            Assert.Equal(orderCheckedOut.CustomerId, saga.Data.CustomerId);
            Assert.False(saga.Data.CardAuthorized);
            Assert.False(saga.Data.CardCharged);
        }

        [Fact]
        public async Task Card_authorization_process_has_been_requested()
        {
            // arrange
            var saga = new PaymentPolicy
            {
                Data = new PaymentPolicyState()
            };
            var context = new TestableMessageHandlerContext();

            var orderCheckedOut = A.Fake<IOrderCheckedOut>();

            A.CallTo(() => orderCheckedOut.CustomerId).Returns(Guid.NewGuid());
            A.CallTo(() => orderCheckedOut.OrderId).Returns(Guid.NewGuid());

            // act
            await saga.Handle(orderCheckedOut, context);

            // assert
            var initiatePaymentProcessing = context.SentMessages
                .Select(sm => sm.Message)
                .OfType<InitiatePaymentProcessing>()
                .SingleOrDefault();

            Assert.NotNull(initiatePaymentProcessing);
            Assert.Equal(initiatePaymentProcessing.OrderId, saga.Data.OrderId);
        }
    }
}
