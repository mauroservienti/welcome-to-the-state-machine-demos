using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ServiceComposer.AspNetCore;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Ticketing.ViewModelComposition.Events;
using Microsoft.AspNetCore.Mvc;

namespace Ticketing.ViewModelComposition
{
    class AvailableTicketsGetHandler : ICompositionRequestsHandler
    {
        readonly Func<Data.TicketingContext> contextFactory;

        public AvailableTicketsGetHandler() : this(() => new Data.TicketingContext())
        {
        }

        internal AvailableTicketsGetHandler(Func<Data.TicketingContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        [HttpGet("/")]
        public async Task Handle(HttpRequest request)
        {
            await using var db = contextFactory();
            var allTickets = await db.Tickets.ToListAsync();
            var availableProductsViewModel = MapToDictionary(allTickets);

            var compositionContext = request.GetCompositionContext();
            await compositionContext.RaiseEvent(new AvailableTicketsLoaded()
            {
                AvailableTicketsViewModel = availableProductsViewModel
            });

            var vm = request.GetComposedResponseModel();
            vm.AvailableTickets = availableProductsViewModel.Values.ToList();
        }

        IDictionary<int, dynamic> MapToDictionary(IEnumerable<Data.Models.Ticket> allTickets)
        {
            var availableTicketsViewModel = new Dictionary<int, dynamic>();

            foreach (var ticket in allTickets)
            {
                dynamic vm = new ExpandoObject();
                vm.TicketId = ticket.Id;
                vm.TicketDescription = ticket.Description;

                availableTicketsViewModel[ticket.Id] = vm;
            }

            return availableTicketsViewModel;
        }
    }
}
