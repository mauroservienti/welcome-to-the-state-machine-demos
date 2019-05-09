using Finance.PaymentGateway.Messages;
using Finance.Service.Messages;
using Finance.Service.Policies;
using NServiceBus.Testing;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Finance.Service.Tests
{
    public class When_payment_processing_is_initiated
    {
        [Fact]
        public async Task Card_authorization_should_be_requested()
        {
            // arrange
            var saga = new PaymentPolicy
            {
                Data = new PaymentPolicyState()
            };
            saga.Data.OrderId = Guid.NewGuid();
            var context = new TestableMessageHandlerContext();

            var initiatePaymentProcessing = new InitiatePaymentProcessing()
            {
                OrderId = saga.Data.OrderId
            };

            // act
            await saga.Handle(initiatePaymentProcessing, context);

            // assert
            var authorizeCardMessage = context.SentMessages
                .Select(sm => sm.Message)
                .OfType<AuthorizeCard>()
                .SingleOrDefault();

            Assert.NotNull(authorizeCardMessage);
            Assert.Equal(initiatePaymentProcessing.OrderId, authorizeCardMessage.OrderId);
            Assert.True(saga.Data.CardAuthorizationRequested);
        }
    }
}
