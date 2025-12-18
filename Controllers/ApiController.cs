using Microsoft.AspNetCore.Mvc;

namespace FitnessCenterManagement.Controllers
{
    public class ApiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
