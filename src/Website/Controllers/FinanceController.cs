using Microsoft.AspNetCore.Mvc;

namespace Website.Controllers
{
    public class FinanceController : Controller
    {
        public IActionResult SelectPaymentMethod()
        {
            return RedirectToAction("Review", "Reservations");
        }
    }
}
