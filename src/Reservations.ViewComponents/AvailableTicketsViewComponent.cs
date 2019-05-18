using Microsoft.AspNetCore.Mvc;

namespace Reservations.ViewComponents
{
    [ViewComponent(Name = "Reservations.ViewComponents.AvailableTickets")]
    public class AvailableTicketsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(dynamic viewModel)
        {
            return View(viewModel);
        }
    }
}