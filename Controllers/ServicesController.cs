using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using Microsoft.AspNetCore.Authorization;

namespace FitnessCenterManagement.Controllers
{
    [Authorize] // Index/Details herkes görsün, Create/Edit/Delete admin
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Services (Herkes görür)
        public async Task<IActionResult> Index()
        {
            return View(await _context.Services.ToListAsync());
        }

        // GET: Services/Details/5 (Herkes görür)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var service = await _context.Services.FirstOrDefaultAsync(m => m.Id == id);
            if (service == null) return NotFound();

            return View(service);
        }

        // GET: Services/Create (Admin)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Services/Create (Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Name,DurationMinutes,Price")] Service service)
        {
            if (ModelState.IsValid)
            {
                _context.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        // ✅ GET: Services/Edit/5 (Admin) -> checkbox trainer listesi
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var service = await _context.Services
                .Include(s => s.Trainers) // many-to-many
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null) return NotFound();

            var allTrainers = await _context.Trainers.ToListAsync();
            var selectedIds = service.Trainers.Select(t => t.Id).ToHashSet();

            var vm = new ServiceEditVM
            {
                Id = service.Id,
                Name = service.Name,
                DurationMinutes = service.DurationMinutes,
                Price = service.Price,
                AllTrainers = allTrainers.Select(t => new TrainerCheckboxVM
                {
                    Id = t.Id,
                    FullName = t.FullName,
                    IsSelected = selectedIds.Contains(t.Id)
                }).ToList(),
                SelectedTrainerIds = selectedIds.ToList()
            };

            return View(vm);
        }

        // ✅ POST: Services/Edit/5 (Admin) -> checkboxlardan gelen trainerlar ile güncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, ServiceEditVM vm)
        {
            if (id != vm.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                // Sayfayı tekrar basmak için trainer listesini yeniden doldur
                var allTrainers = await _context.Trainers.ToListAsync();
                var selected = vm.SelectedTrainerIds.ToHashSet();

                vm.AllTrainers = allTrainers.Select(t => new TrainerCheckboxVM
                {
                    Id = t.Id,
                    FullName = t.FullName,
                    IsSelected = selected.Contains(t.Id)
                }).ToList();

                return View(vm);
            }

            var service = await _context.Services
                .Include(s => s.Trainers)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null) return NotFound();

            // Service alanlarını güncelle
            service.Name = vm.Name;
            service.DurationMinutes = vm.DurationMinutes;
            service.Price = vm.Price;

            // Trainer ilişkilerini güncelle
            service.Trainers.Clear();

            var trainers = await _context.Trainers
                .Where(t => vm.SelectedTrainerIds.Contains(t.Id))
                .ToListAsync();

            foreach (var t in trainers)
                service.Trainers.Add(t);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Services/Delete/5 (Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var service = await _context.Services.FirstOrDefaultAsync(m => m.Id == id);
            if (service == null) return NotFound();

            return View(service);
        }

        // POST: Services/Delete/5 (Admin)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
