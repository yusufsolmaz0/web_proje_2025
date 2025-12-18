using Microsoft.AspNetCore.Mvc;

namespace FitnessCenterManagement.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
