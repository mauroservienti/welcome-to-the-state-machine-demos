using Finance.Messages.Commands;
using Finance.PaymentGateway.Messages;
using Finance.Service.Messages;
using NServiceBus;
using Reservations.Messages.Events;
using System;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace Finance.Service.Policies
{
    class PaymentPolicy : Saga<PaymentPolicyState>,
        IAmStartedByMessages<IReservationCheckedout>,
        IAmStartedByMessages<InitializeReservationPaymentPolicy>,
        IHandleMessages<InitiatePaymentProcessing>,
        IHandleMessages<CardAuthorizedResponse>,
        IHandleTimeouts<CardAuthorizationTimeout>,
        IHandleMessages<IOrderCreated>,
        IHandleMessages<CardChargedResponse>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<PaymentPolicyState> mapper)
        {
            mapper.ConfigureMapping<IReservationCheckedout>(m => m.ReservationId).ToSaga(s => s.ReservationId);
            mapper.ConfigureMapping<InitializeReservationPaymentPolicy>(m => m.ReservationId).ToSaga(s => s.ReservationId);
            mapper.ConfigureMapping<InitiatePaymentProcessing>(m => m.ReservationId).ToSaga(s => s.ReservationId);
            mapper.ConfigureMapping<IOrderCreated>(m => m.ReservationId).ToSaga(s => s.ReservationId);
        }

        public Task Handle(IReservationCheckedout message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Reservation '{message.ReservationId}' checked out.", Color.Green);

            Data.ReservationId = message.ReservationId;
            Data.ReservationCheckedOut = true;

            return InitiatePaymentProcessing(context);
        }

        public Task Handle(InitializeReservationPaymentPolicy message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Adding payment method '{message.PaymentMethodId}' to reservation '{message.ReservationId}'.", Color.Green);

            Data.PaymentMethodId = message.PaymentMethodId;
            Data.PaymentMethodSet = true;

            return InitiatePaymentProcessing(context);
        }

        Task InitiatePaymentProcessing(IMessageHandlerContext context)
        {
            Console.WriteLine($"Going to check if payment processing can be started.", Color.Green);

            if (Data.ReservationCheckedOut && Data.PaymentMethodSet)
            {
                Console.WriteLine($"All information required to start the payment process have been collected.", Color.Green);

                return context.SendLocal(new InitiatePaymentProcessing()
                {
                    ReservationId = Data.ReservationId
                });
            }

            Console.WriteLine($"Not all information are available to start the payment process.", Color.Yellow);

            return Task.CompletedTask;
        }

        public async Task Handle(InitiatePaymentProcessing message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Ready to start the payment process for reservation '{message.ReservationId}'. First step is to authorize the credit card with Id '{Data.PaymentMethodId}'.", Color.Green);

            Data.CardAuthorizationRequested = true;

            await context.Send(new AuthorizeCard()
            {
                ReservationId = Data.ReservationId,
                PaymentMethodId = Data.PaymentMethodId
            });

            Console.WriteLine($"Authorization requested.", Color.Green);
        }

        public async Task Handle(CardAuthorizedResponse message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Card authorized.", Color.Green);

            /*
             * Intentionally ignoring authorization failures
             * --------------------------------------------------
             * The demo starts from the assumption that card
             * authorization never fails. To handle such scenario
             * a couple more messages are needed and one more
             * interaction with Reservation to release tickets.
             * Or a timeout in Reservation to handle payment
             * missing events.
             */
            Data.CardAuthorized = true;
            Data.PaymentAuthorizationId = message.AuthorizationId;

            await RequestTimeout<CardAuthorizationTimeout>(context, TimeSpan.FromMinutes(20));
            await context.Publish(new PaymentAuthorized() { ReservationId = Data.ReservationId });

            Console.WriteLine($"CardAuthorizationTimeout set, and PaymentAutorized published.", Color.Green);
        }

        public Task Timeout(CardAuthorizationTimeout state, IMessageHandlerContext context)
        {
            Console.WriteLine($"Card Authorization timed out, going to release money.", Color.Green);

            MarkAsComplete();
            return context.Send(new ReleaseCardAuthorization()
            {
                AuthorizationId = Data.PaymentAuthorizationId,
                ReservationId = Data.ReservationId
            });
        }

        public Task Handle(IOrderCreated message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Order '{message.OrderId}' for reservation '{message.ReservationId}' created, going to confirm card payment.", Color.Green);

            Data.OrderId = message.OrderId;
            return context.Send(new ChargeCard()
            {
                AuthorizationId = Data.PaymentAuthorizationId,
                ReservationId = Data.ReservationId
            });
        }

        public Task Handle(CardChargedResponse message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Card charged, I'm done. Publishing PaymentSucceeded event.", Color.Green);

            MarkAsComplete();
            return context.Publish(new PaymentSucceeded()
            {
                OrderId = Data.OrderId,
                ReservationId = Data.ReservationId
            });
        }
    }

    class CardAuthorizationTimeout { }

    class PaymentPolicyState : ContainSagaData
    {
        public Guid ReservationId { get; set; }
        public bool CardCharged { get; set; }
        public bool CardAuthorized { get; set; }
        public bool CardAuthorizationRequested { get; set; }
        public int PaymentMethodId { get; set; }
        public bool ReservationCheckedOut { get; set; }
        public bool PaymentMethodSet { get; set; }
        public Guid PaymentAuthorizationId { get; set; }
        public Guid OrderId { get; set; }
    }
}