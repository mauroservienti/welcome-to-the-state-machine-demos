using Microsoft.AspNetCore.Mvc;
using NServiceBus;
using Reservations.Messages.Commands;
using System;
using System.Threading.Tasks;

namespace Reservations.ViewComponents.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly IMessageSession messageSession;

        public ReservationsController(IMessageSession messageSession)
        {
            this.messageSession = messageSession;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Reserve(int id)
        {
            var message = new ReserveTicket()
            {
                TicketId = id,
                ReservationId = new Guid(ControllerContext.HttpContext.Request.Cookies["reservation-id"])
            };
            //WARN: destination is hardcoded to reduce demo complexity. In a real project it should not.
            await messageSession.Send("Reservations.Service", message);

            return RedirectToAction("Added");
        }

        public IActionResult Added()
        {
            return View();
        }
    }
}
