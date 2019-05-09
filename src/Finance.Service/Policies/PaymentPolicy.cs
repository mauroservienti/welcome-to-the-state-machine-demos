using System;
using System.Threading.Tasks;
using Finance.Service.Messages;
using NServiceBus;
using Ticketing.Messages.Events;

namespace Finance.Service.Policies
{
    class PaymentPolicy : Saga<PaymentPolicyState>,
        IAmStartedByMessages<IOrderCheckedOut>
    {
        public Task Handle(IOrderCheckedOut message, IMessageHandlerContext context)
        {
            Data.OrderId = message.OrderId;
            Data.CustomerId = message.CustomerId;

            return context.SendLocal(new AuthorizeCard() { OrderId = Data.OrderId });
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<PaymentPolicyState> mapper)
        {
            mapper.ConfigureMapping<IOrderCheckedOut>(m => m.OrderId).ToSaga(s => s.OrderId);
        }
    }

    class PaymentPolicyState : ContainSagaData
    {
        public Guid OrderId{ get; set; }
        public Guid CustomerId { get; set; }
        public bool CardCharged { get; internal set; }
        public bool CardAuthorized { get; internal set; }
    }
}