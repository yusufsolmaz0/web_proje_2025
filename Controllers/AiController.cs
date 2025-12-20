using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitnessCenterManagement.Data;
using FitnessCenterManagement.Services;
using FitnessCenterManagement.Models;

namespace FitnessCenterManagement.Controllers
{
    [Authorize]
    public class AiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GroqPlanService _ai;

        public AiController(ApplicationDbContext context, GroqPlanService ai)
        {
            _context = context;
            _ai = ai;
        }

        [HttpGet]
        public IActionResult Index()
        {
            
            return View(new AiRequestVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AiRequestVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            
            double heightM = model.HeightCm / 100.0;
            double bmi = 0;

            if (heightM > 0)
                bmi = model.WeightKg / (heightM * heightM);

            
            var suggestionText = await _ai.GeneratePlanAsync(
                model.HeightCm,
                model.WeightKg,
                model.Goal,
                model.ExtraInfo
            );

            
            ViewBag.Suggestion = suggestionText;
            ViewBag.Bmi = bmi;

            

            return View(model);
        }

        
    }
}
