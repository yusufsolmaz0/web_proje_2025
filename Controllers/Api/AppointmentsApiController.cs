using Microsoft.AspNetCore.Mvc;

namespace FitnessCenterManagement.Controllers.Api
{
    public class AppointmentsApiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
