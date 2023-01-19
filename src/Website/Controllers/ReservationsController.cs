using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers
{
    public class ReservationsController : Controller
    {
        [HttpGet("/reservations")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("/reservations/reserve/{id}")]
        public IActionResult Reserve(int id)
        {
            return RedirectToAction("Added");
        }

        [HttpGet("/reservations/added")]
        public IActionResult Added()
        {
            return View();
        }

        [HttpGet("/reservations/review")]
        public IActionResult Review()
        {
            return View();
        }

        [HttpPost("/reservations/finalize")]
        public IActionResult Finalize()
        {
            return RedirectToAction("Review");
        }

        [HttpPost("/reservations/checkout")]
        public IActionResult Checkout()
        {
            return RedirectToAction("CheckedOut");
        }

        [HttpGet("/reservations/checkedout")]
        public IActionResult CheckedOut()
        {
            return View();
        }
    }
}
