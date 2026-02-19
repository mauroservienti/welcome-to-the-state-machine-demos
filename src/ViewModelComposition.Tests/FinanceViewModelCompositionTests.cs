using Finance.Data.Models;
using Finance.Messages.Commands;
using Finance.ViewModelComposition;
using Microsoft.EntityFrameworkCore;
using Reservations.ViewModelComposition.Events;
using ServiceComposer.AspNetCore;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Ticketing.ViewModelComposition.Events;
using Xunit;

namespace ViewModelComposition.Tests
{
    public class FinanceViewModelCompositionTests
    {
        static Finance.Data.FinanceContext CreateFinanceContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<Finance.Data.FinanceContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new Finance.Data.FinanceContext(options);
        }

        // ReservationsReservePostHandler

        [Fact]
        public async Task ReservationsReservePostHandler_SendsStoreReservedTicketCommand()
        {
            var reservationId = Guid.NewGuid();
            var ticketId = 5;
            var session = new FakeTransactionalSession();
            var handler = new ReservationsReservePostHandler(session);

            var (request, _, _) = TestRequestBuilder.Build(
                cookies => cookies["reservation-id"] = reservationId.ToString(),
                routeValues => routeValues["id"] = ticketId.ToString());

            await handler.Handle(request);

            var msg = session.SentMessages.OfType<StoreReservedTicket>().SingleOrDefault();
            Assert.NotNull(msg);
            Assert.Equal(reservationId, msg.ReservationId);
            Assert.Equal(ticketId, msg.TicketId);
        }

        // ReservationsCheckoutPostHandler

        [Fact]
        public async Task ReservationsCheckoutPostHandler_SendsInitializeReservationPaymentPolicyCommand()
        {
            var reservationId = Guid.NewGuid();
            var paymentMethodId = 2;
            var session = new FakeTransactionalSession();
            var handler = new ReservationsCheckoutPostHandler(session);

            var (request, _, _) = TestRequestBuilder.Build(cookies =>
            {
                cookies["reservation-id"] = reservationId.ToString();
                cookies["reservation-payment-method-id"] = paymentMethodId.ToString();
            });

            await handler.Handle(request);

            var msg = session.SentMessages.OfType<InitializeReservationPaymentPolicy>().SingleOrDefault();
            Assert.NotNull(msg);
            Assert.Equal(reservationId, msg.ReservationId);
            Assert.Equal(paymentMethodId, msg.PaymentMethodId);
        }

        // ReservationsCheckedoutGetHandler

        [Fact]
        public async Task ReservationsCheckedoutGetHandler_ExpiresPaymentMethodCookie()
        {
            var handler = new ReservationsCheckedoutGetHandler();
            var (request, _, _) = TestRequestBuilder.Build();

            await handler.Handle(request);

            var setCookie = request.HttpContext.Response.Headers["Set-Cookie"].ToString();
            Assert.Contains("reservation-payment-method-id=", setCookie);
            Assert.Contains("expires=", setCookie, StringComparison.OrdinalIgnoreCase);
        }

        // ReservationsFinalizePostHandler

        [Fact]
        public async Task ReservationsFinalizePostHandler_StoresPaymentMethodCookie()
        {
            var handler = new ReservationsFinalizePostHandler();
            var (request, _, _) = TestRequestBuilder.Build(
                configureForm: form => form["PaymentMethod"] = "1");

            await handler.Handle(request);

            var setCookie = request.HttpContext.Response.Headers["Set-Cookie"].ToString();
            Assert.Contains("reservation-payment-method-id=1", setCookie);
        }

        // AvailableTicketsLoadedSubscriber

        [Fact]
        public async Task AvailableTicketsLoadedSubscriber_EnrichesWithTicketPrice()
        {
            var dbName = Guid.NewGuid().ToString("N");
            await using (var db = CreateFinanceContext(dbName))
            {
                db.TicketPrices.Add(new TicketPrice { Id = 1, Price = 49.99m });
                await db.SaveChangesAsync();
            }

            dynamic ticketVm = new ExpandoObject();
            var availableVm = new Dictionary<int, dynamic> { [1] = ticketVm };
            var @event = new AvailableTicketsLoaded { AvailableTicketsViewModel = availableVm };

            CompositionEventHandler<AvailableTicketsLoaded>? capturedHandler = null;
            var publisher = new FakeCompositionEventsPublisher<AvailableTicketsLoaded>(h => capturedHandler = h);

            var subscriber = new AvailableTicketsLoadedSubscriber(() => CreateFinanceContext(dbName));
            subscriber.Subscribe(publisher);

            var (request, _, _) = TestRequestBuilder.Build();
            await capturedHandler!(@event, request);

            Assert.Equal(49.99m, (decimal)ticketVm.TicketPrice);
        }

        // ReservedTicketsLoadedSubscriber

        [Fact]
        public async Task ReservedTicketsLoadedSubscriber_EnrichesWithPricesAndTotalAndPaymentMethods()
        {
            var dbName = Guid.NewGuid().ToString("N");
            var reservationId = Guid.NewGuid();

            await using (var db = CreateFinanceContext(dbName))
            {
                db.TicketPrices.Add(new TicketPrice { Id = 1, Price = 96m });
                db.ReservedTickets.Add(new Finance.Data.Models.ReservedTicket
                {
                    ReservationId = reservationId, TicketId = 1
                });
                db.PaymentMethods.Add(new PaymentMethod { Id = 1, Description = "Visa" });
                await db.SaveChangesAsync();
            }

            dynamic reservation = new ExpandoObject();
            reservation.Id = reservationId;
            dynamic ticketVm = new ExpandoObject();
            var reservedTicketsVm = new Dictionary<int, dynamic> { [1] = ticketVm };
            var @event = new ReservedTicketsLoaded
            {
                ReservedTicketsViewModel = reservedTicketsVm,
                Reservation = reservation
            };

            CompositionEventHandler<ReservedTicketsLoaded>? capturedHandler = null;
            var publisher = new FakeCompositionEventsPublisher<ReservedTicketsLoaded>(h => capturedHandler = h);

            var subscriber = new ReservedTicketsLoadedSubscriber(() => CreateFinanceContext(dbName));
            subscriber.Subscribe(publisher);

            var (request, vm, _) = TestRequestBuilder.Build();
            await capturedHandler!(@event, request);

            Assert.Equal(96m, (decimal)ticketVm.TicketPrice);
            Assert.Equal(96m, (decimal)ticketVm.TotalPrice);
            Assert.Equal(96m, (decimal)reservation.TotalPrice);

            var paymentMethods = (System.Collections.Generic.List<PaymentMethod>)vm.PaymentMethods;
            Assert.Single(paymentMethods);
            Assert.Equal(1, paymentMethods[0].Id);
        }

        // ReviewReservedTicketsLoadedSubscriber

        [Fact]
        public async Task ReviewReservedTicketsLoadedSubscriber_EnrichesWithPricesAndSelectedPaymentMethod()
        {
            var dbName = Guid.NewGuid().ToString("N");
            var reservationId = Guid.NewGuid();

            await using (var db = CreateFinanceContext(dbName))
            {
                db.TicketPrices.Add(new TicketPrice { Id = 1, Price = 50m });
                db.ReservedTickets.Add(new Finance.Data.Models.ReservedTicket
                {
                    ReservationId = reservationId, TicketId = 1
                });
                db.PaymentMethods.Add(new PaymentMethod { Id = 1, Description = "MasterCard" });
                await db.SaveChangesAsync();
            }

            dynamic reservation = new ExpandoObject();
            reservation.Id = reservationId;
            dynamic ticketVm = new ExpandoObject();
            var reservedTicketsVm = new Dictionary<int, dynamic> { [1] = ticketVm };
            var @event = new ReservedTicketsLoaded
            {
                ReservedTicketsViewModel = reservedTicketsVm,
                Reservation = reservation
            };

            CompositionEventHandler<ReservedTicketsLoaded>? capturedHandler = null;
            var publisher = new FakeCompositionEventsPublisher<ReservedTicketsLoaded>(h => capturedHandler = h);

            var subscriber = new ReviewReservedTicketsLoadedSubscriber(() => CreateFinanceContext(dbName));
            subscriber.Subscribe(publisher);

            var (request, vm, _) = TestRequestBuilder.Build(
                cookies => cookies["reservation-payment-method-id"] = "1");
            await capturedHandler!(@event, request);

            Assert.Equal(50m, (decimal)ticketVm.TicketPrice);
            Assert.Equal(50m, (decimal)ticketVm.TotalPrice);
            Assert.Equal(50m, (decimal)reservation.TotalPrice);

            var paymentMethod = (PaymentMethod)vm.PaymentMethod;
            Assert.Equal(1, paymentMethod.Id);
            Assert.Equal("MasterCard", paymentMethod.Description);
        }
    }
}
