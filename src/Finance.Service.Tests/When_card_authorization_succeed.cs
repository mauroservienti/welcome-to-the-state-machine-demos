using Finance.Messages.Events;
using Finance.PaymentGateway.Messages;
using Finance.Service.Policies;
using NServiceBus.Testing;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Finance.Service.Tests
{
    public class When_card_authorization_succeed
    {
        [Fact]
        public async Task Payment_authorized_event_should_be_published()
        {
            // arrange
            var saga = new PaymentPolicy
            {
                Data = new PaymentPolicyState()
            };
            saga.Data.OrderId = Guid.NewGuid();
            var context = new TestableMessageHandlerContext();

            var authorizeCardResponse = new AuthorizeCardResponse()
            {
                Succeeded = true,
                OrderId = saga.Data.OrderId
            };

            // act
            await saga.Handle(authorizeCardResponse, context);

            // assert
            var paymentAuthorized = context.PublishedMessages
                .Select(sm => sm.Message)
                .OfType<IPaymentAuthorized>()
                .SingleOrDefault();

            Assert.NotNull(paymentAuthorized);
            Assert.Equal(saga.Data.OrderId, paymentAuthorized.OrderId);
            Assert.True(saga.Data.CardAuthorized);
        }
    }
}
