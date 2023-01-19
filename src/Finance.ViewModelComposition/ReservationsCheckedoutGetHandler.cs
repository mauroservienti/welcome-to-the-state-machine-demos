using Microsoft.AspNetCore.Http;
using ServiceComposer.AspNetCore;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Finance.ViewModelComposition
{
    class ReservationsCheckedoutGetHandler : ICompositionRequestsHandler
    {
        [HttpGet("/reservations/checkedout")]
        public Task Handle(HttpRequest request)
        {
            /*
             * delete the reservation payment method cookie
             */
            var response = request.HttpContext.Response;
            response.Cookies.Append(
                key: "reservation-payment-method-id",
                value: "",
                options: new CookieOptions()
                {
                    Expires = DateTimeOffset.Now.AddHours(-1)
                });

            return Task.CompletedTask;
        }
    }
}
