using Microsoft.AspNetCore.Mvc;

namespace Reservations.ViewComponents.Controllers
{
    public class ReservationsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Reserve(int id)
        {
            return RedirectToAction("Added");
        }

        public IActionResult Added()
        {
            return View();
        }
    }
}
