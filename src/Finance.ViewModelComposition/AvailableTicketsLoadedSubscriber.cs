using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using ServiceComposer.AspNetCore;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Ticketing.ViewModelComposition.Events;

namespace Finance.ViewModelComposition
{
    class AvailableTicketsLoadedSubscriber : ICompositionEventsSubscriber
    {
        readonly Func<Data.FinanceContext> contextFactory;

        public AvailableTicketsLoadedSubscriber() : this(() => new Data.FinanceContext())
        {
        }

        internal AvailableTicketsLoadedSubscriber(Func<Data.FinanceContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        [HttpGet("/")]
        public void Subscribe(ICompositionEventsPublisher publisher)
        {
            publisher.Subscribe<AvailableTicketsLoaded>(async (@event, request) =>
            {
                var ids = @event.AvailableTicketsViewModel.Keys.ToArray();
                await using var db = contextFactory();
                var ticketPrices = await db.TicketPrices
                    .Where(ticketPrice => ids.Contains(ticketPrice.Id))
                    .ToListAsync();

                foreach (var ticketPrice in ticketPrices)
                {
                    @event.AvailableTicketsViewModel[(int)ticketPrice.Id].TicketPrice = ticketPrice.Price;
                }
            });
        }
    }
}
