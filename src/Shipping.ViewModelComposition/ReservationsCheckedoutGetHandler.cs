using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ServiceComposer.AspNetCore;
using System;
using System.Threading.Tasks;

namespace Shipping.ViewModelComposition
{
    class ReservationsCheckedoutGetHandler : IHandleRequests
    {
        public bool Matches(RouteData routeData, string httpVerb, HttpRequest request)
        {
            var controller = (string)routeData.Values["controller"];
            var action = (string)routeData.Values["action"];

            return HttpMethods.IsGet(httpVerb)
                   && controller.ToLowerInvariant() == "reservations"
                   && action.ToLowerInvariant() == "checkedout";
        }

        public Task Handle(string requestId, dynamic vm, RouteData routeData, HttpRequest request)
        {
            /* 
             * delete the reservation delivery option cookie
             */
            var response = request.HttpContext.Response;
            response.Cookies.Append(
                    key: "reservation-delivery-option-id",
                    value: "",
                    options: new CookieOptions()
                    {
                        Expires = DateTimeOffset.Now.AddHours(-1)
                    });

            return Task.CompletedTask;
        }
    }
}
