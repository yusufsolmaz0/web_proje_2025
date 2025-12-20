using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // GET: /api/availabilityapi/available-trainers-for-service?serviceId=1&date=2025-12-20T13:00
        [HttpGet("available-trainers-for-service")]
        public IActionResult GetAvailableTrainersForService([FromQuery] int serviceId, [FromQuery] DateTime date)
        {
            if (serviceId <= 0)
                return BadRequest("serviceId zorunlu ve 0'dan büyük olmalı.");

            if (date == default)
                return BadRequest("date zorunlu. Örn: 2025-12-20T13:00");

            // 1) O saatte dolu trainerlar
            var busyTrainerIds = _context.Appointments
                .Where(a =>
                    a.StartTime.Year == date.Year &&
                    a.StartTime.Month == date.Month &&
                    a.StartTime.Day == date.Day &&
                    a.StartTime.Hour == date.Hour &&
                    a.StartTime.Minute == date.Minute)
                .Select(a => a.TrainerId)
                .Distinct()
                .ToList();

            // 2) O servisi yapan + dolu olmayan trainerlar
            var available = _context.Trainers
                .Include(t => t.Services)
                .Where(t => t.Services.Any(s => s.Id == serviceId))
                .Where(t => !busyTrainerIds.Contains(t.Id))
                .Select(t => new
                {
                    id = t.Id,
                    fullName = t.FullName
                })
                .ToList();

            return Ok(available);
        }
        // GET: /api/availabilityapi/available-times?trainerId=1&serviceId=2&date=2025-12-20
        [HttpGet("available-times")]
        public IActionResult GetAvailableTimes([FromQuery] int trainerId, [FromQuery] int serviceId, [FromQuery] DateTime date)
        {
            if (trainerId <= 0) return BadRequest("trainerId zorunlu.");
            if (serviceId <= 0) return BadRequest("serviceId zorunlu.");
            if (date == default) return BadRequest("date zorunlu. Örn: 2025-12-20");

            // Trainer bu servisi yapıyor mu?
            var canDoService = _context.Trainers
                .Include(t => t.Services)
                .Any(t => t.Id == trainerId && t.Services.Any(s => s.Id == serviceId));

            if (!canDoService)
                return Ok(new string[] { }); // boş döndür

            // O gün (date) 12:00 - 18:00 arası dolu saatler
            var dayStart = date.Date.AddHours(12); // 12:00
            var dayEnd = date.Date.AddHours(18);   // 18:00

            var busyTimes = _context.Appointments
                .Where(a => a.TrainerId == trainerId && a.StartTime >= dayStart && a.StartTime <= dayEnd)
                .Select(a => a.StartTime)
                .ToList();

            // Slotları üret (12:00, 13:00, ... 18:00)
            var slots = new List<string>();
            for (int hour = 12; hour <= 18; hour++)
            {
                var slot = date.Date.AddHours(hour);

                bool isBusy = busyTimes.Any(b =>
                    b.Year == slot.Year &&
                    b.Month == slot.Month &&
                    b.Day == slot.Day &&
                    b.Hour == slot.Hour &&
                    b.Minute == slot.Minute);

                if (!isBusy)
                    slots.Add(slot.ToString("HH:mm"));
            }

            return Ok(slots);
        }

    }
}
