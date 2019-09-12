using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers
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

        public IActionResult Review()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Confirm()
        {
            return RedirectToAction("CheckedOut");
        }

        [HttpPost]
        public IActionResult Checkout()
        {
            return RedirectToAction("CheckedOut");
        }

        public IActionResult CheckedOut()
        {
            //we should delete the reservation cookie and the 
            //payment-id one so to let infrastructure create
            //a new reservation

            return View();
        }
    }
}
