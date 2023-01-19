using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ServiceComposer.AspNetCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Shipping.ViewModelComposition
{
    class ReservationsFinalizePostHandler : ICompositionRequestsHandler
    {
        [HttpPost("/reservations/finalize")]
        public Task Handle(HttpRequest request)
        {
            var response = request.HttpContext.Response;
            response.Cookies.Append("reservation-delivery-option-id", request.Form["DeliveryOption"]);

            return Task.CompletedTask;
        }
    }
}
