using Finance.Messages.Commands;
using Finance.PaymentGateway.Messages;
using Finance.Service.Messages;
using NServiceBus;
using Reservations.Messages.Events;
using System;
using System.Threading.Tasks;

namespace Finance.Service.Policies
{
    class PaymentPolicy : Saga<PaymentPolicyState>,
        IAmStartedByMessages<IReservationCheckedout>,
        IAmStartedByMessages<InitializeReservationPaymentPolicy>,
        IHandleMessages<InitiatePaymentProcessing>,
        IHandleMessages<CardAuthorizedResponse>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<PaymentPolicyState> mapper)
        {
            mapper.ConfigureMapping<IReservationCheckedout>(m => m.ReservationId).ToSaga(s => s.ReservationId);
            mapper.ConfigureMapping<InitializeReservationPaymentPolicy>(m => m.ReservationId).ToSaga(s => s.ReservationId);
            mapper.ConfigureMapping<InitiatePaymentProcessing>(m => m.ReservationId).ToSaga(s => s.ReservationId);
        }

        public Task Handle(IReservationCheckedout message, IMessageHandlerContext context)
        {
            Data.ReservationId = message.ReservationId;
            Data.ReservationCheckedOut = true;

            return InitiatePaymentProcessing(context);
        }

        public Task Handle(InitializeReservationPaymentPolicy message, IMessageHandlerContext context)
        {
            Data.PaymentMethodId = message.PaymentMethodId;
            Data.PaymentMethodSet = true;

            return InitiatePaymentProcessing(context);
        }

        Task InitiatePaymentProcessing(IMessageHandlerContext context)
        {
            if(Data.ReservationCheckedOut && Data.PaymentMethodSet)
            {
                return context.SendLocal(new InitiatePaymentProcessing()
                {
                    ReservationId = Data.ReservationId
                });
            }

            return Task.CompletedTask;
        }

        public Task Handle(InitiatePaymentProcessing message, IMessageHandlerContext context)
        {
            Data.CardAuthorizationRequested = true;

            return context.Send(new AuthorizeCard() { ReservationId = Data.ReservationId });
        }

        public Task Handle(CardAuthorizedResponse message, IMessageHandlerContext context)
        {
            /*
             * Intentionally ignoring authorization failures
             * --------------------------------------------------
             * The demo starts from the assumption that card
             * authorization never fails. To handle such scenario
             * a couple more messages are needed and one more
             * interaction with Reservation to release tickets.
             */
            Data.CardAuthorized = true;

            return context.Publish(new PaymentAuthorized() { ReservationId = Data.ReservationId });
        }
    }

    class PaymentPolicyState : ContainSagaData
    {
        public Guid ReservationId { get; set; }
        public bool CardCharged { get; set; }
        public bool CardAuthorized { get; set; }
        public bool CardAuthorizationRequested { get; set; }
        public int PaymentMethodId { get; set; }
        public bool ReservationCheckedOut { get; set; }
        public bool PaymentMethodSet { get; set; }
    }
}