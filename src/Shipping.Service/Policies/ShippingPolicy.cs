using Finance.Messages.Events;
using NServiceBus;
using Reservations.Messages.Events;
using Shipping.Data;
using Shipping.Messages.Commands;
using Shipping.Service.Messages;
using System;
using System.Drawing;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace Shipping.Service.Policies
{
    class ShippingPolicy : Saga<ShippingPolicyState>,
        IAmStartedByMessages<InitializeReservationShippingPolicy>,
        IAmStartedByMessages<IOrderCreated>,
        IAmStartedByMessages<IPaymentSucceeded>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ShippingPolicyState> mapper)
        {
            mapper.MapSaga(saga => saga.ReservationId)
                .ToMessage<InitializeReservationShippingPolicy>(m => m.ReservationId)
                .ToMessage<IPaymentSucceeded>(m => m.ReservationId)
                .ToMessage<IOrderCreated>(m => m.ReservationId);
        }

        public Task Handle(InitializeReservationShippingPolicy message, IMessageHandlerContext context)
        {
            Console.WriteLine($"DeliveryOption {message.DeliveryOption} set for reservation '{message.ReservationId}'. Verifying if shipment can started...", Color.Green);

            Data.DeliveryOption = message.DeliveryOption;
            Data.DeliveryOptionDefined = true;

            return StartShipmentProcessIfEverythingIsOk(context);
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
            if (Data.OrderCreated && Data.PaymentSucceeded && Data.DeliveryOptionDefined)
            {
                MarkAsComplete();

                switch (Data.DeliveryOption)
                {
                    case DeliveryOptions.ShipAtHome:
                        /*
                         * Send a message locally to the courier
                         * gateway to request a pick-up. And finally
                         * publish the OrderShipped event.
                         */
                        Console.WriteLine($"Order '{Data.OrderId}' will be shipped ASAP...", Color.Green);
                        return context.Publish(new OrderShipped()
                        {
                            ShipmentId = Guid.NewGuid(),
                            OrderId = Data.OrderId,
                            ReservationId = Data.ReservationId
                        });

                    case DeliveryOptions.CollectAtTheVenue:
                        /*
                         * Send a message locally to kick-off another
                         * saga to handle the physical delivery, this
                         * second saga (outside the scope of this demo)
                         * is responsible to handle the delivery to
                         * venues.
                         * The complexity is that an order can contain
                         * tickets for different events that should be
                         * delivered at different venues at different
                         * times.
                         * 
                         * In this demo this message is never handled and
                         * goes nowhere.
                         */
                        Console.WriteLine($"Order '{Data.OrderId}' will be shipped at the venue...", Color.Green);
                        return context.SendLocal(new StoreReservationForVenueDelivery()
                        {
                            OrderId = Data.OrderId,
                            ReservationId = Data.ReservationId
                        });
                }
            }

            Console.WriteLine($"Shipment for Order '{Data.OrderId}' cannot be started yet...", Color.Yellow);

            return Task.CompletedTask;
        }
    }

    class ShippingPolicyState : ContainSagaData
    {
        public Guid ReservationId { get; set; }
        public Guid OrderId { get; set; }
        public bool PaymentSucceeded { get; set; }
        public bool OrderCreated { get; set; }
        public bool DeliveryOptionDefined { get; set; }
        public DeliveryOptions DeliveryOption { get; set; }
    }
}