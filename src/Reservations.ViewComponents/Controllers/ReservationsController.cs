using Microsoft.AspNetCore.Mvc;
using NServiceBus;
using Reservations.Messages.Commands;
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
        
        [HttpPost]
        public async Task<IActionResult> Reserve(int id)
        {
            //WARN: destination is hardcoded to reduce demo complexity. In a real project it should not.
            await messageSession.Send("Reservations.Service", new ReserveTicket() { TicketId = id });

            return View();
        }
    }
}
