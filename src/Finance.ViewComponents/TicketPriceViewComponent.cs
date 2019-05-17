using Microsoft.AspNetCore.Mvc;

namespace Finance.ViewComponents
{
    [ViewComponent(Name = "Finance.ViewComponents.TicketPrice")]
    public class TicketPriceViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(dynamic viewModel)
        {
            return View(viewModel);
        }
    }
}