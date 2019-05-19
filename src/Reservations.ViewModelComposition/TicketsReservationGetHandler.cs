using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ServiceComposer.AspNetCore;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using Reservations.ViewModelComposition.Events;

namespace Reservations.ViewModelComposition
{
    class TicketsReservationGetHandler : IHandleRequests
    {
        public bool Matches(RouteData routeData, string httpVerb, HttpRequest request)
        {
            var controller = (string)routeData.Values["controller"];
            var action = (string)routeData.Values["action"];

            return HttpMethods.IsGet(httpVerb)
                   && controller.ToLowerInvariant() == "reservations"
                   && (action.ToLowerInvariant() == "index" || action.ToLowerInvariant() == "review")
                   && !routeData.Values.ContainsKey("id");
        }

        public async Task Handle(string requestId, dynamic vm, RouteData routeData, HttpRequest request)
        {
            if (!request.Cookies.ContainsKey("reservation-id"))
            {
                vm.Reservation = null;
                return;
            }

            var reservationId = new Guid(request.Cookies["reservation-id"]);
            using (var db = Data.ReservationsContext.Create())
            {
                var reservation = await db.Reservations
                    .Where(r => r.Id == reservationId)
                    .Include(r => r.ReservedTickets)
                    .SingleOrDefaultAsync();

                if (reservation == null)
                {
                    vm.Reservation = null;
                    return;
                }

                vm.Reservation = new ExpandoObject();
                vm.Reservation.Id = reservationId;

                IEnumerable<(int TicketId, int Quantity)> reservedTickeks = reservation.ReservedTickets
                    .GroupBy(t => t.TicketId)
                    .Select(g => (g.Key, g.Count()));

                var reservedTicketsViewModel = MapToDictionary(reservedTickeks);

                await vm.RaiseEvent(new ReservedTicketsLoaded()
                {
                    Reservation = vm.Reservation, 
                    ReservedTicketsViewModel = reservedTicketsViewModel
                });

                vm.Reservation.ReservedTickets = reservedTicketsViewModel.Values.ToList();
            }
        }

        IDictionary<int, dynamic> MapToDictionary(IEnumerable<(int TicketId, int Quantity)> reservedTickets)
        {
            var reservedTicketsViewModel = new Dictionary<int, dynamic>();

            foreach (var reservedTicket in reservedTickets)
            {
                dynamic vm = new ExpandoObject();
                vm.TicketId = reservedTicket.TicketId;
                vm.Quantity = reservedTicket.Quantity;

                reservedTicketsViewModel[reservedTicket.TicketId] = vm;
            }

            return reservedTicketsViewModel;
        }
    }
}
