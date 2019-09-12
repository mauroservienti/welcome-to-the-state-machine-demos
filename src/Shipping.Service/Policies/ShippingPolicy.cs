using Finance.Messages.Events;
using NServiceBus;
using Reservations.Messages.Events;
using Shipping.Service.Messages;
using System;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace Shipping.Service.Policies
{
    class ShippingPolicy : Saga<ShippingPolicyState>,
        IAmStartedByMessages<IOrderCreated>,
        IAmStartedByMessages<IPaymentSucceeded>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ShippingPolicyState> mapper)
        {
            mapper.ConfigureMapping<IPaymentSucceeded>(m => m.ReservationId).ToSaga(s => s.ReservationId);
            mapper.ConfigureMapping<IOrderCreated>(m => m.ReservationId).ToSaga(s => s.ReservationId);
        }

        public Task Handle(IPaymentSucceeded message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Payment for reservation '{message.ReservationId}' succeeded. Verifying if shipment can started...", Color.Green);

            Data.PaymentSucceeded = true;

            return StartShipmentProcessIfEverythingIsOk(context);
        }

        public Task Handle(IOrderCreated message, IMessageHandlerContext context)
        {
            Console.WriteLine($"Order '{message.OrderId}' for reservation '{message.ReservationId}' created. Verifying if shipment can started...", Color.Green);

            Data.ReservationId = message.ReservationId;
            Data.OrderId = message.OrderId;
            Data.OrderCreated = true;

            return StartShipmentProcessIfEverythingIsOk(context);
        }

        private Task StartShipmentProcessIfEverythingIsOk(IMessageHandlerContext context)
        {
            if (Data.OrderCreated && Data.PaymentSucceeded)
            {
                /*
                 * Send a message locally to kick-off another
                 * saga to handle the physical delivery, this
                 * second saga (outside the scope of this demo)
                 * is responsible to handle the handshaking with
                 * delivery couriers.
                 */
                MarkAsComplete();
                return context.Publish(new OrderShipped()
                {
                    ShipmentId = Guid.NewGuid(),
                    OrderId = Data.OrderId,
                    ReservationId = Data.ReservationId
                }); ;
            }

            return Task.CompletedTask;
        }
    }

    class ShippingPolicyState : ContainSagaData
    {
        public Guid ReservationId { get; set; }
        public Guid OrderId { get; set; }
        public bool PaymentSucceeded { get; set; }
        public bool OrderCreated { get; set; }
    }
}