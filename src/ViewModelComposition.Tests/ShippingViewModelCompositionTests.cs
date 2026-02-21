using Reservations.ViewModelComposition.Events;
using ServiceComposer.AspNetCore;
using Shipping.Data;
using Shipping.Messages.Commands;
using Shipping.ViewModelComposition;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ViewModelComposition.Tests
{
    public class ShippingViewModelCompositionTests
    {
        // ReservationsCheckoutPostHandler

        [Fact]
        public async Task ReservationsCheckoutPostHandler_SendsInitializeReservationShippingPolicyCommand()
        {
            var reservationId = Guid.NewGuid();
            var session = new FakeTransactionalSession();
            var handler = new ReservationsCheckoutPostHandler(session);

            var (request, _, _) = TestRequestBuilder.Build(cookies =>
            {
                cookies["reservation-id"] = reservationId.ToString();
                cookies["reservation-delivery-option-id"] = DeliveryOptions.ShipAtHome.ToString();
            });

            await handler.Handle(request);

            var msg = session.SentMessages.OfType<InitializeReservationShippingPolicy>().SingleOrDefault();
            Assert.NotNull(msg);
            Assert.Equal(reservationId, msg.ReservationId);
            Assert.Equal(DeliveryOptions.ShipAtHome, msg.DeliveryOption);
        }

        [Fact]
        public async Task ReservationsCheckoutPostHandler_CollectAtTheVenue_SendsCorrectDeliveryOption()
        {
            var reservationId = Guid.NewGuid();
            var session = new FakeTransactionalSession();
            var handler = new ReservationsCheckoutPostHandler(session);

            var (request, _, _) = TestRequestBuilder.Build(cookies =>
            {
                cookies["reservation-id"] = reservationId.ToString();
                cookies["reservation-delivery-option-id"] = DeliveryOptions.CollectAtTheVenue.ToString();
            });

            await handler.Handle(request);

            var msg = session.SentMessages.OfType<InitializeReservationShippingPolicy>().SingleOrDefault();
            Assert.NotNull(msg);
            Assert.Equal(DeliveryOptions.CollectAtTheVenue, msg.DeliveryOption);
        }

        // ReservationsCheckedoutGetHandler

        [Fact]
        public async Task ReservationsCheckedoutGetHandler_ExpiresDeliveryOptionCookie()
        {
            var handler = new ReservationsCheckedoutGetHandler();
            var (request, _, _) = TestRequestBuilder.Build();

            await handler.Handle(request);

            var setCookie = request.HttpContext.Response.Headers["Set-Cookie"].ToString();
            Assert.Contains("reservation-delivery-option-id=", setCookie);
            Assert.Contains("expires=", setCookie, StringComparison.OrdinalIgnoreCase);
        }

        // ReservationsFinalizePostHandler

        [Fact]
        public async Task ReservationsFinalizePostHandler_StoresDeliveryOptionCookie()
        {
            var handler = new ReservationsFinalizePostHandler();
            var (request, _, _) = TestRequestBuilder.Build(
                configureForm: form => form["DeliveryOption"] = DeliveryOptions.ShipAtHome.ToString());

            await handler.Handle(request);

            var setCookie = request.HttpContext.Response.Headers["Set-Cookie"].ToString();
            Assert.Contains("reservation-delivery-option-id=ShipAtHome", setCookie);
        }

        // ReservedTicketsLoadedSubscriber

        [Fact]
        public async Task ReservedTicketsLoadedSubscriber_AddsDeliveryOptionsToViewModel()
        {
            dynamic reservation = new ExpandoObject();
            reservation.Id = Guid.NewGuid();
            var @event = new ReservedTicketsLoaded
            {
                ReservedTicketsViewModel = new Dictionary<int, dynamic>(),
                Reservation = reservation
            };

            CompositionEventHandler<ReservedTicketsLoaded>? capturedHandler = null;
            var publisher = new FakeCompositionEventsPublisher<ReservedTicketsLoaded>(h => capturedHandler = h);

            var subscriber = new ReservedTicketsLoadedSubscriber();
            subscriber.Subscribe(publisher);

            var (request, vm, _) = TestRequestBuilder.Build();
            await capturedHandler!(@event, request);

            var options = (List<dynamic>)vm.DeliveryOptions;
            Assert.Equal(2, options.Count);
            Assert.Contains(options, o => o.Id == DeliveryOptions.ShipAtHome);
            Assert.Contains(options, o => o.Id == DeliveryOptions.CollectAtTheVenue);
        }

        // ReviewReservedTicketsLoadedSubscriber

        [Fact]
        public async Task ReviewReservedTicketsLoadedSubscriber_ShipAtHome_SetsDeliveryOptionOnViewModel()
        {
            dynamic reservation = new ExpandoObject();
            reservation.Id = Guid.NewGuid();
            var @event = new ReservedTicketsLoaded
            {
                ReservedTicketsViewModel = new Dictionary<int, dynamic>(),
                Reservation = reservation
            };

            CompositionEventHandler<ReservedTicketsLoaded>? capturedHandler = null;
            var publisher = new FakeCompositionEventsPublisher<ReservedTicketsLoaded>(h => capturedHandler = h);

            var subscriber = new ReviewReservedTicketsLoadedSubscriber();
            subscriber.Subscribe(publisher);

            var (request, vm, _) = TestRequestBuilder.Build(
                cookies => cookies["reservation-delivery-option-id"] = DeliveryOptions.ShipAtHome.ToString());
            await capturedHandler!(@event, request);

            Assert.Equal(DeliveryOptions.ShipAtHome, (DeliveryOptions)vm.DeliveryOption.Id);
            Assert.Equal("Ship at Home.", (string)vm.DeliveryOption.Description);
        }

        [Fact]
        public async Task ReviewReservedTicketsLoadedSubscriber_CollectAtTheVenue_SetsDeliveryOptionOnViewModel()
        {
            dynamic reservation = new ExpandoObject();
            reservation.Id = Guid.NewGuid();
            var @event = new ReservedTicketsLoaded
            {
                ReservedTicketsViewModel = new Dictionary<int, dynamic>(),
                Reservation = reservation
            };

            CompositionEventHandler<ReservedTicketsLoaded>? capturedHandler = null;
            var publisher = new FakeCompositionEventsPublisher<ReservedTicketsLoaded>(h => capturedHandler = h);

            var subscriber = new ReviewReservedTicketsLoadedSubscriber();
            subscriber.Subscribe(publisher);

            var (request, vm, _) = TestRequestBuilder.Build(
                cookies => cookies["reservation-delivery-option-id"] = DeliveryOptions.CollectAtTheVenue.ToString());
            await capturedHandler!(@event, request);

            Assert.Equal(DeliveryOptions.CollectAtTheVenue, (DeliveryOptions)vm.DeliveryOption.Id);
            Assert.Equal("Collect at the Venue.", (string)vm.DeliveryOption.Description);
        }
    }
}
