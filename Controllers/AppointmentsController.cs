using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using Microsoft.AspNetCore.Authorization;

namespace FitnessCenterManagement.Controllers
{
    [Authorize] // Giriş yapan herkes görebilsin (üye randevu alacak)
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            var list = await _context.Appointments
                .Join(_context.Members, a => a.MemberId, m => m.Id, (a, m) => new { a, m })
                .Join(_context.Trainers, x => x.a.TrainerId, t => t.Id, (x, t) => new { x.a, x.m, t })
                .Join(_context.Services, x => x.a.ServiceId, s => s.Id, (x, s) => new AppointmentListVM
                {
                    Id = x.a.Id,
                    MemberName = x.m.FullName,
                    TrainerName = x.t.FullName,
                    ServiceName = s.Name,
                    StartTime = x.a.StartTime,
                    IsApproved = x.a.IsApproved
                })
                .OrderByDescending(x => x.StartTime)
                .ToListAsync();

            return View(list);
        }


        // GET: Appointments/Create
        public IActionResult Create()
        {
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName");
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name");
            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TrainerId,ServiceId,StartTime")] Appointment appointment)
        {
            // ✅ Member otomatik atanacak (ilk member)
            var member = _context.Members.FirstOrDefault();
            if (member == null)
            {
                ModelState.AddModelError("", "Member bulunamadı. Önce /Members sayfasından 1 tane member ekle.");

                ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
                ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name", appointment.ServiceId);
                return View(appointment);
            }

            appointment.MemberId = member.Id;

            // ✅ Çakışma kontrolü: aynı eğitmen, aynı saat
            bool conflict = _context.Appointments.Any(a =>
                a.TrainerId == appointment.TrainerId &&
                a.StartTime == appointment.StartTime);

            if (conflict)
            {
                ModelState.AddModelError("", "Bu eğitmen bu saatte dolu. Başka bir saat seç.");
            }

            if (ModelState.IsValid)
            {
                appointment.IsApproved = false; // admin onaylayacak
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Hata varsa dropdownlar tekrar dolsun
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name", appointment.ServiceId);

            return View(appointment);
        }

        // GET: Appointments/Edit/5
        [Authorize(Roles = "Admin")] // sadece admin düzenlesin
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name", appointment.ServiceId);

            return View(appointment);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MemberId,TrainerId,ServiceId,StartTime,IsApproved")] Appointment appointment)
        {
            if (id != appointment.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name", appointment.ServiceId);

            return View(appointment);
        }

        // GET: Appointments/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments.FirstOrDefaultAsync(m => m.Id == id);
            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ✅ Admin Onay
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            appt.IsApproved = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ✅ Admin Reddet (sil)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            var appt = await _context.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            _context.Appointments.Remove(appt);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
