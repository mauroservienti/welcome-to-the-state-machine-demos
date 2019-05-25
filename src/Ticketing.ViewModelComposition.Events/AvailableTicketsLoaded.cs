using System.Collections.Generic;

namespace Ticketing.ViewModelComposition.Events
{
    public class AvailableTicketsLoaded
    {
        public IDictionary<int, dynamic> AvailableTicketsViewModel { get; set; }
    }
}
