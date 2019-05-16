using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ticketing.ViewComponents.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Ticketing.ViewComponents.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            using (var db = Data.TicketingContext.Create())
            {
                var allTickets = await db.Tickets.ToListAsync();

                return View(allTickets);
            }
        }
    }
}
