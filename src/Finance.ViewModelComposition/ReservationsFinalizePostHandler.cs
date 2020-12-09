using Microsoft.AspNetCore.Http;
using ServiceComposer.AspNetCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Finance.ViewModelComposition
{
    class ReservationsFinalizePostHandler : ICompositionRequestsHandler
    {
        [HttpPost("/reservations/finalize")]
        public Task Handle(HttpRequest request)
        {
            var response = request.HttpContext.Response;
            response.Cookies.Append("reservation-payment-method-id", request.Form["PaymentMethod"]);

            return Task.CompletedTask;
        }
    }
}
