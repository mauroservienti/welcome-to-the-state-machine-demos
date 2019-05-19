using System.Collections.Generic;

namespace Reservations.ViewModelComposition.Events
{
    public class ReservedTicketsLoaded
    {
        public IDictionary<int, dynamic> ReservedTicketsViewModel { get; set; }
        public dynamic Reservation { get; set; }
    }
}
