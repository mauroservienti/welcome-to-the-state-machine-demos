using System;
using System.Linq;
using System.Threading.Tasks;
using Finance.Data;
using Finance.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using NServiceBus;
using ServiceComposer.AspNetCore;

namespace Finance.ViewModelComposition
{
    class SelectPaymentMethodPostHandler : IHandleRequests
    {
        public bool Matches(RouteData routeData, string httpVerb, HttpRequest request)
        {
            var controller = (string)routeData.Values["controller"];
            var action = (string)routeData.Values["action"];

            return HttpMethods.IsPost(httpVerb)
                   && controller.ToLowerInvariant() == "finance"
                   && action.ToLowerInvariant() == "selectpaymentmethod";
        }

        public async Task Handle(string requestId, dynamic vm, RouteData routeData, HttpRequest request)
        {
            var reservationId = new Guid(request.Cookies["reservation-id"]);
            using (var db = FinanceContext.Create())
            {
                var reservationPaymentMethod = await db.ReservationsPaymentMethod
                    .Where(rpm => rpm.Id == reservationId)
                    .SingleOrDefaultAsync();

                if (reservationPaymentMethod == null)
                {
                    reservationPaymentMethod = new ReservationPaymentMethod()
                    {
                        Id = reservationId
                    };
                    db.ReservationsPaymentMethod.Add(reservationPaymentMethod);
                }

                reservationPaymentMethod.PaymentMethod = int.Parse(request.Form["PaymentMethod"]);

                await db.SaveChangesAsync();
            }
        }
    }
}
