using Finance.PaymentGateway.Messages;
using Finance.Service.Messages;
using NServiceBus;
using System;
using System.Threading.Tasks;
using Ticketing.Messages.Events;

namespace Finance.Service.Policies
{
    class PaymentPolicy : Saga<PaymentPolicyState>,
        IAmStartedByMessages<IOrderCheckedOut>,
        IHandleMessages<InitiatePaymentProcessing>,
        IHandleMessages<AuthorizeCardResponse>
    {
        public Task Handle(IOrderCheckedOut message, IMessageHandlerContext context)
        {
            Data.OrderId = message.OrderId;
            Data.CustomerId = message.CustomerId;

            return context.SendLocal(new InitiatePaymentProcessing() { OrderId = Data.OrderId });
        }

        public Task Handle(InitiatePaymentProcessing message, IMessageHandlerContext context)
        {
            Data.CardAuthorizationRequested = true;

            return context.Send(new AuthorizeCard() { OrderId = Data.OrderId });
        }

        public Task Handle(AuthorizeCardResponse message, IMessageHandlerContext context)
        {
            Data.CardAuthorized = message.Succeeded;

            return context.Publish(new PaymentAuthorized() { OrderId = Data.OrderId });
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<PaymentPolicyState> mapper)
        {
            mapper.ConfigureMapping<IOrderCheckedOut>(m => m.OrderId).ToSaga(s => s.OrderId);
            mapper.ConfigureMapping<InitiatePaymentProcessing>(m => m.OrderId).ToSaga(s => s.OrderId);
        }
    }

    class PaymentPolicyState : ContainSagaData
    {
        public Guid OrderId{ get; set; }
        public Guid CustomerId { get; set; }
        public bool CardCharged { get; set; }
        public bool CardAuthorized { get; set; }
        public bool CardAuthorizationRequested { get; set; }
    }
}