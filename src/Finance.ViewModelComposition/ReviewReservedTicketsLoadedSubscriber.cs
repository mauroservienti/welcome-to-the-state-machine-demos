﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Reservations.ViewModelComposition.Events;
using ServiceComposer.AspNetCore;
using System;
using System.Linq;

namespace Finance.ViewModelComposition
{
    class ReviewReservedTicketsLoadedSubscriber : ISubscribeToCompositionEvents
    {
        public bool Matches(RouteData routeData, string httpVerb, HttpRequest request)
        {
            var controller = (string)routeData.Values["controller"];
            var action = (string)routeData.Values["action"];

            return HttpMethods.IsGet(httpVerb)
                   && controller.ToLowerInvariant() == "reservations"
                   && action.ToLowerInvariant() == "review"
                   && !routeData.Values.ContainsKey("id");
        }

        public void Subscribe(IPublishCompositionEvents publisher)
        {
            publisher.Subscribe<ReservedTicketsLoaded>(async (requestId, viewModel, @event, douteData, httpRequest) =>
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
                    var selectedPaymentMethodId = int.Parse(httpRequest.Cookies["reservation-payment-method-id"]);
                    var paymentMethod = await db.PaymentMethods
                        .Where(pm => pm.Id == selectedPaymentMethodId)
                        .SingleAsync();
                    viewModel.PaymentMethod = paymentMethod;
                }
            });
        }
    }
}
