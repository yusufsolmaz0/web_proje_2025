using FitnessCenterManagement.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace FitnessCenterManagement.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrainersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_context.Trainers.ToList());
        }
    }
}
