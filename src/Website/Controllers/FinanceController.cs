using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
