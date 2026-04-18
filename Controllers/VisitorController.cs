using Microsoft.AspNetCore.Mvc;
using SocietyManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace SocietyManagementSystem.Controllers
{
    public class VisitorController : Controller
    {
        private readonly AppDbContext _context;
        public VisitorController(AppDbContext context) { _context = context; }
        private bool IsAdmin() => HttpContext.Session.GetString("UserRole") == "Admin";

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var visitors = await _context.Visitors.Include(v => v.Resident).OrderByDescending(v => v.VisitDate).ToListAsync();
            return View(visitors);
        }

        public async Task<IActionResult> Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.Residents = await _context.Residents.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Visitor visitor)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            visitor.VisitDate = DateTime.Now;
            visitor.Status = "Inside";
            _context.Visitors.Add(visitor);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> MarkLeft(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var visitor = await _context.Visitors.FindAsync(id);
            if (visitor != null) { visitor.Status = "Left"; await _context.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var visitor = await _context.Visitors.FindAsync(id);
            if (visitor != null) { _context.Visitors.Remove(visitor); await _context.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }
    }
}