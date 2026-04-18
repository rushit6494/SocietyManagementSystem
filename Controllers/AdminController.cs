using Microsoft.AspNetCore.Mvc;
using SocietyManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace SocietyManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // Check Admin Session
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // Dashboard
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.TotalResidents = await _context.Residents.CountAsync();
            ViewBag.TotalComplaints = await _context.Complaints.CountAsync();
            ViewBag.PendingComplaints = await _context.Complaints
                .Where(c => c.Status == "Pending").CountAsync();
            ViewBag.TotalBills = await _context.MaintenanceBills.CountAsync();
            ViewBag.UnpaidBills = await _context.MaintenanceBills
                .Where(b => b.IsPaid == false).CountAsync();
            ViewBag.TotalVisitors = await _context.Visitors.CountAsync();
            ViewBag.TotalNotices = await _context.Notices.CountAsync();
            ViewBag.TotalParking = await _context.ParkingSlots.CountAsync();

            return View();
        }
    }
}