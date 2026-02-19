using Finance.Messages.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NServiceBus.TransactionalSession;
using ServiceComposer.AspNetCore;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Finance.ViewModelComposition
{
    class ReservationsReservePostHandler : ICompositionRequestsHandler
    {
        private readonly ITransactionalSession transactionalSession;

        public ReservationsReservePostHandler(ITransactionalSession transactionalSession)
        {
            this.transactionalSession = transactionalSession;
        }

        [HttpPost("/reservations/reserve/{id}")]
        public Task Handle(HttpRequest request)
        {
            /*
             * In a production environment if multiple services are interested in the
             * same post request the handling logic is much more complex than what we
             * are doing in this demo. In this demo both Finance and Reservations need
             * to handle the POST to /reservations/reserve. The implementation assumes
             * that the host/infrastructure never fails, which is not the case in a
             * production environment. In order to make this part safe, which is not the
             * scope of this demo asynchronous messaging should be introduced earlier in
             * the processing pipeline.
             *
             * More information: https://milestone.topics.it/2019/05/02/safety-first.html
             */

            var message = new StoreReservedTicket()
            {
                TicketId = int.Parse((string)request.HttpContext.GetRouteValue("id")),
                ReservationId = new Guid(request.Cookies["reservation-id"])
            };

            /*
             * WARN: destination is hard-coded to reduce demo complexity.
             * In a production environment routing should be configured
             * at startup by the host/infrastructure.
             */
            return transactionalSession.Send("Finance.Service", message, request.HttpContext.RequestAborted);
        }
    }
}
