using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ServiceComposer.AspNetCore;
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
        [HttpGet("/")]
        public async Task Handle(HttpRequest request)
        {
            using (var db = Data.TicketingContext.Create())
            {
                var allTickets = await db.Tickets.ToListAsync();
                var availableProductsViewModel = MapToDictionary(allTickets);

                var vm = request.GetComposedResponseModel();
                await vm.RaiseEvent(new AvailableTicketsLoaded()
                {
                    AvailableTicketsViewModel = availableProductsViewModel
                });

                vm.AvailableTickets = availableProductsViewModel.Values.ToList();
            }
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
