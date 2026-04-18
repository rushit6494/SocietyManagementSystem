using Microsoft.AspNetCore.Mvc;
using SocietyManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace SocietyManagementSystem.Controllers
{
    public class ParkingController : Controller
    {
        private readonly AppDbContext _context;
        public ParkingController(AppDbContext context) { _context = context; }
        private bool IsAdmin() => HttpContext.Session.GetString("UserRole") == "Admin";

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var slots = await _context.ParkingSlots.Include(p => p.Resident).ToListAsync();
            return View(slots);
        }

        public async Task<IActionResult> Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.Residents = await _context.Residents.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ParkingSlot slot)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (slot.ResidentId != null) slot.IsOccupied = true;
            _context.ParkingSlots.Add(slot);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var slot = await _context.ParkingSlots.FindAsync(id);
            if (slot != null) { _context.ParkingSlots.Remove(slot); await _context.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }
    }
}