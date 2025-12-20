using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using Microsoft.AspNetCore.Authorization;


namespace FitnessCenterManagement.Controllers
{
    [Authorize]
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
            // Temel sorgu (JOIN'li)
            var query =
                from a in _context.Appointments
                join m in _context.Members on a.MemberId equals m.Id
                join t in _context.Trainers on a.TrainerId equals t.Id
                join s in _context.Services on a.ServiceId equals s.Id
                select new
                {
                    Appointment = a,
                    MemberName = m.FullName,
                    TrainerName = t.FullName,
                    ServiceName = s.Name
                };

            
            if (!User.IsInRole("Admin"))
            {
                var userEmail = User.Identity?.Name;

                if (string.IsNullOrWhiteSpace(userEmail))
                    return View(new List<AppointmentListVM>());

                var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == userEmail);
                if (member == null)
                    return View(new List<AppointmentListVM>());

                query = query.Where(x => x.Appointment.MemberId == member.Id);
            }

            
            var list = await query
                .OrderByDescending(x => x.Appointment.StartTime)
                .Select(x => new AppointmentListVM
                {
                    Id = x.Appointment.Id,
                    MemberName = x.MemberName,
                    TrainerName = x.TrainerName,
                    ServiceName = x.ServiceName,
                    StartTime = x.Appointment.StartTime,
                    IsApproved = x.Appointment.IsApproved
                })
                .ToListAsync();

            return View(list);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var appt = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appt == null) return NotFound();

            // Admin değilse sadece kendi randevusunu görebilsin
            if (!User.IsInRole("Admin"))
            {
                var userEmail = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(userEmail)) return Forbid();

                var member = await _context.Members.FirstOrDefaultAsync(m => m.Email == userEmail);
                if (member == null) return Forbid();

                if (appt.MemberId != member.Id) return Forbid();
            }

            // View, AppointmentListVM bekleyecek (birazdan Details view'ını da veriyorum)
            var vm = await (
                from a in _context.Appointments
                join m in _context.Members on a.MemberId equals m.Id
                join t in _context.Trainers on a.TrainerId equals t.Id
                join s in _context.Services on a.ServiceId equals s.Id
                where a.Id == id
                select new AppointmentListVM
                {
                    Id = a.Id,
                    MemberName = m.FullName,
                    TrainerName = t.FullName,
                    ServiceName = s.Name,
                    StartTime = a.StartTime,
                    IsApproved = a.IsApproved
                }
            ).FirstOrDefaultAsync();

            if (vm == null) return NotFound();

            return View(vm);
        }






        // GET: Appointments/Create
        public IActionResult Create()
        {
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name");
            ViewData["TrainerId"] = new SelectList(Enumerable.Empty<SelectListItem>());
            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TrainerId,ServiceId,StartTime")] Appointment appointment)
        {
            
            var email = User?.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Giriş yapan kullanıcı bulunamadı (email boş).");

                ViewData["TrainerId"] = new SelectList(_context.Trainers, "Id", "FullName", appointment.TrainerId);
                ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name", appointment.ServiceId);
                return View(appointment);
            }

            
            var member = _context.Members.FirstOrDefault(m => m.Email == email);

            
            if (member == null)
            {
                member = new Member
                {
                    FullName = email,
                    Email = email
                };

                _context.Members.Add(member);
                await _context.SaveChangesAsync();
            }

            appointment.MemberId = member.Id;


            
            bool conflict = _context.Appointments.Any(a =>
                a.TrainerId == appointment.TrainerId &&
                a.StartTime == appointment.StartTime);

            if (conflict)
            {
                ModelState.AddModelError("", "Bu eğitmen bu saatte dolu. Başka bir saat seç.");
            }

            if (ModelState.IsValid)
            {
                appointment.IsApproved = false;
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
        [Authorize(Roles = "Admin")]
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
