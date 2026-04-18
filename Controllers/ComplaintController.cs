using Microsoft.AspNetCore.Mvc;
using SocietyManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace SocietyManagementSystem.Controllers
{
    public class ComplaintController : Controller
    {
        private readonly AppDbContext _context;
        public ComplaintController(AppDbContext context) { _context = context; }
        private bool IsAdmin() => HttpContext.Session.GetString("UserRole") == "Admin";

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var complaints = await _context.Complaints.Include(c => c.Resident).OrderByDescending(c => c.CreatedAt).ToListAsync();
            return View(complaints);
        }

        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint != null)
            {
                complaint.Status = status;
                if (status == "Resolved") complaint.ResolvedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint != null) { _context.Complaints.Remove(complaint); await _context.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }
    }
}