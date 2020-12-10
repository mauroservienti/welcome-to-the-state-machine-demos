using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Reservations.ViewModelComposition.Events;
using ServiceComposer.AspNetCore;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Reservations.ViewModelComposition
{
    class TicketsReservationGetHandler : ICompositionRequestsHandler
    {
        [HttpGet("/reservations/index")]
        [HttpGet("/reservations/review")]
        public async Task Handle(HttpRequest request)
        {
            var vm = request.GetComposedResponseModel();

            if (!request.Cookies.ContainsKey("reservation-id"))
            {
                vm.Reservation = null;
                return;
            }

            var reservationId = new Guid(request.Cookies["reservation-id"]);
            await using var db = Data.ReservationsContext.Create();
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

            IEnumerable<(int TicketId, int Quantity)> reservedTickets = reservation.ReservedTickets
                .GroupBy(t => t.TicketId)
                .Select(g => (g.Key, g.Count()));

            var reservedTicketsViewModel = MapToDictionary(reservedTickets);

            await vm.RaiseEvent(new ReservedTicketsLoaded()
            {
                Reservation = vm.Reservation,
                ReservedTicketsViewModel = reservedTicketsViewModel
            });

            vm.Reservation.ReservedTickets = reservedTicketsViewModel.Values.ToList();
        }

        static IDictionary<int, dynamic> MapToDictionary(IEnumerable<(int TicketId, int Quantity)> reservedTickets)
        {
            var reservedTicketsViewModel = new Dictionary<int, dynamic>();

            foreach ((int ticketId, int quantity) in reservedTickets)
            {
                dynamic vm = new ExpandoObject();
                vm.TicketId = ticketId;
                vm.Quantity = quantity;

                reservedTicketsViewModel[ticketId] = vm;
            }

            return reservedTicketsViewModel;
        }
    }
}
