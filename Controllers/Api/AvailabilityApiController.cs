using Microsoft.AspNetCore.Mvc;
using FitnessCenterManagement.Data;

namespace FitnessCenterManagement.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvailabilityApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AvailabilityApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /api/availabilityapi/available-trainers?date=2025-12-20T13:00
        [HttpGet("available-trainers")]
        public IActionResult GetAvailableTrainers([FromQuery] DateTime date)
        {
            // LINQ filtre: o saatte randevusu olmayan trainerlar
            var busyTrainerIds = _context.Appointments
                .Where(a => a.StartTime == date)
                .Select(a => a.TrainerId)
                .Distinct()
                .ToList();

            var available = _context.Trainers
                .Where(t => !busyTrainerIds.Contains(t.Id))
                .Select(t => new
                {
                    t.Id,
                    t.FullName,
                    t.Specialty
                })
                .ToList();

            return Ok(available);
        }
    }
}
