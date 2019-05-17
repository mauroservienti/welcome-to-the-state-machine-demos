using Microsoft.AspNetCore.Mvc;

namespace Ticketing.ViewComponents.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
