using Microsoft.AspNetCore.Mvc;
using SocietyManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace SocietyManagementSystem.Controllers
{
    public class ResidentController : Controller
    {
        private readonly AppDbContext _context;

        public ResidentController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId") != null;
        }

        // GET: All Residents
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var residents = await _context.Residents
                .Include(r => r.User)
                .ToListAsync();
            return View(residents);
        }

        // GET: Create Resident
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        // POST: Create Resident
        [HttpPost]
        public async Task<IActionResult> Create(Resident resident, string email, string password)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Pehla User banavo
            var user = new User
            {
                Name = resident.Name,
                Email = email,
                Password = password,
                Role = "Resident"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Pachhi Resident banavo
            resident.UserId = user.Id;
            resident.Email = email;
            _context.Residents.Add(resident);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: Edit Resident
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var resident = await _context.Residents.FindAsync(id);
            if (resident == null) return NotFound();
            return View(resident);
        }

        // POST: Edit Resident
        [HttpPost]
        public async Task<IActionResult> Edit(Resident resident)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            _context.Residents.Update(resident);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // POST: Delete Resident
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var resident = await _context.Residents
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resident != null)
            {
                _context.Residents.Remove(resident);
                if (resident.User != null)
                    _context.Users.Remove(resident.User);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}