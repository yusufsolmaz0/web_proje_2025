using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using Microsoft.AspNetCore.Authorization;

namespace FitnessCenterManagement.Controllers
{
    [Authorize]
    public class TrainersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Trainers (Herkes görür)
        public async Task<IActionResult> Index()
        {
            return View(await _context.Trainers.ToListAsync());
        }

        // GET: Trainers/Details/5 (Herkes görür)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var trainer = await _context.Trainers
                .Include(t => t.Services)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (trainer == null) return NotFound();

            return View(trainer);
        }

        // GET: Trainers/Create (Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var services = await _context.Services.ToListAsync();

            var vm = new TrainerEditVM
            {
                AllServices = services.Select(s => new ServiceCheckboxVM
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsSelected = false
                }).ToList()
            };

            return View(vm);
        }

        // POST: Trainers/Create (Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(TrainerEditVM vm)
        {
            if (!ModelState.IsValid)
            {
                // checkboxları tekrar doldur
                var services = await _context.Services.ToListAsync();
                var selected = vm.SelectedServiceIds.ToHashSet();

                vm.AllServices = services.Select(s => new ServiceCheckboxVM
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsSelected = selected.Contains(s.Id)
                }).ToList();

                return View(vm);
            }

            var selectedServices = await _context.Services
                .Where(s => vm.SelectedServiceIds.Contains(s.Id))
                .ToListAsync();

            var trainer = new Trainer
            {
                FullName = vm.FullName,
                Services = selectedServices
            };

            _context.Trainers.Add(trainer);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Trainers/Edit/5 (Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var trainer = await _context.Trainers
                .Include(t => t.Services)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null) return NotFound();

            var services = await _context.Services.ToListAsync();
            var selectedIds = trainer.Services.Select(s => s.Id).ToHashSet();

            var vm = new TrainerEditVM
            {
                Id = trainer.Id,
                FullName = trainer.FullName,
                SelectedServiceIds = selectedIds.ToList(),
                AllServices = services.Select(s => new ServiceCheckboxVM
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsSelected = selectedIds.Contains(s.Id)
                }).ToList()
            };

            return View(vm);
        }

        // POST: Trainers/Edit/5 (Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, TrainerEditVM vm)
        {
            if (id != vm.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                var services = await _context.Services.ToListAsync();
                var selected = vm.SelectedServiceIds.ToHashSet();

                vm.AllServices = services.Select(s => new ServiceCheckboxVM
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsSelected = selected.Contains(s.Id)
                }).ToList();

                return View(vm);
            }

            var trainer = await _context.Trainers
                .Include(t => t.Services)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null) return NotFound();

            trainer.FullName = vm.FullName;

            trainer.Services.Clear();
            var selectedServices = await _context.Services
                .Where(s => vm.SelectedServiceIds.Contains(s.Id))
                .ToListAsync();

            foreach (var s in selectedServices)
                trainer.Services.Add(s);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Trainers/Delete/5 (Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var trainer = await _context.Trainers.FirstOrDefaultAsync(m => m.Id == id);
            if (trainer == null) return NotFound();

            return View(trainer);
        }

        // POST: Trainers/Delete/5 (Admin)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer != null)
            {
                _context.Trainers.Remove(trainer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
