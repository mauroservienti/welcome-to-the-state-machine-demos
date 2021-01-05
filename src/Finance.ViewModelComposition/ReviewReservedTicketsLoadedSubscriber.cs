using Microsoft.EntityFrameworkCore;
using Reservations.ViewModelComposition.Events;
using ServiceComposer.AspNetCore;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Finance.ViewModelComposition
{
    class ReviewReservedTicketsLoadedSubscriber : ICompositionEventsSubscriber
    {
        [HttpGet("/reservations/review")]
        public void Subscribe(ICompositionEventsPublisher publisher)
        {
            publisher.Subscribe<ReservedTicketsLoaded>(async (@event,request) =>
            {
                var ids = @event.ReservedTicketsViewModel.Keys.ToArray();
                using (var db = Data.FinanceContext.Create())
                {
                    var ticketPrices = await db.TicketPrices
                        .Where(ticketPrice => ids.Contains(ticketPrice.Id))
                        .ToListAsync();

                    Guid reservationId = @event.Reservation.Id;
                    var reservedTickets =
                    (
                        await db.ReservedTickets
                            .Where(r => r.ReservationId == reservationId)
                            .ToListAsync()
                    )
                    .GroupBy(t => t.TicketId)
                    .ToDictionary(g=>g.Key, g=>g.Count());

                    var reservationTotalPrice = 0m;

                    foreach (var ticketPrice in ticketPrices)
                    {
                        var ticketId = (int)ticketPrice.Id;
                        var reservedTicketViewModel = @event.ReservedTicketsViewModel[ticketId];

                        var currentReservedTicketQuantity = reservedTickets[ticketId];
                        var currentReservedTicketTotalPrice = ticketPrice.Price * currentReservedTicketQuantity;

                        reservationTotalPrice += currentReservedTicketTotalPrice;

                        reservedTicketViewModel.TicketPrice = ticketPrice.Price;
                        reservedTicketViewModel.TotalPrice = currentReservedTicketTotalPrice;
                    }

                    @event.Reservation.TotalPrice = reservationTotalPrice;

                    /*
                     * it's a demo, production code should check for cookie existence
                     */
                    var selectedPaymentMethodId = int.Parse(request.Cookies["reservation-payment-method-id"]);
                    var paymentMethod = await db.PaymentMethods
                        .Where(pm => pm.Id == selectedPaymentMethodId)
                        .SingleAsync();

                    var viewModel = request.GetComposedResponseModel();
                    viewModel.PaymentMethod = paymentMethod;
                }
            });
        }
    }
}
