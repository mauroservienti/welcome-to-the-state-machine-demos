using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using ServiceComposer.AspNetCore;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Ticketing.ViewModelComposition.Events;

namespace Ticketing.ViewModelComposition
{
    class AvailableTicketsGetHandler : IHandleRequests
    {
        public bool Matches(RouteData routeData, string httpVerb, HttpRequest request)
        {
            var controller = (string)routeData.Values["controller"];
            var action = (string)routeData.Values["action"];

            return HttpMethods.IsGet(httpVerb)
                   && controller.ToLowerInvariant() == "home"
                   && action.ToLowerInvariant() == "index"
                   && !routeData.Values.ContainsKey("id");
        }

        public async Task Handle(string requestId, dynamic vm, RouteData routeData, HttpRequest request)
        {
            using (var db = Data.TicketingContext.Create())
            {
                var allTickets = await db.Tickets.ToListAsync();
                var availableProductsViewModel = MapToDictionary(allTickets);

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
