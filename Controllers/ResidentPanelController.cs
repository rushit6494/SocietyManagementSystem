using Microsoft.AspNetCore.Mvc;
using SocietyManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace SocietyManagementSystem.Controllers
{
    public class ResidentPanelController : Controller
    {
        private readonly AppDbContext _context;
        public ResidentPanelController(AppDbContext context) { _context = context; }

        private bool IsResident() => HttpContext.Session.GetString("UserRole") == "Resident";
        private int GetUserId() => HttpContext.Session.GetInt32("UserId") ?? 0;

        // Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!IsResident()) return RedirectToAction("Login", "Account");

            var resident = await _context.Residents
                .FirstOrDefaultAsync(r => r.UserId == GetUserId());

            if (resident == null) return RedirectToAction("Login", "Account");

            ViewBag.Resident = resident;
            ViewBag.TotalBills = await _context.MaintenanceBills.CountAsync(b => b.ResidentId == resident.Id);
            ViewBag.UnpaidBills = await _context.MaintenanceBills.CountAsync(b => b.ResidentId == resident.Id && !b.IsPaid);
            ViewBag.TotalComplaints = await _context.Complaints.CountAsync(c => c.ResidentId == resident.Id);
            ViewBag.PendingComplaints = await _context.Complaints.CountAsync(c => c.ResidentId == resident.Id && c.Status == "Pending");

            return View();
        }

        // My Bills
        public async Task<IActionResult> MyBills()
        {
            if (!IsResident()) return RedirectToAction("Login", "Account");
            var resident = await _context.Residents.FirstOrDefaultAsync(r => r.UserId == GetUserId());
            if (resident == null) return RedirectToAction("Login", "Account");

            var bills = await _context.MaintenanceBills
                .Where(b => b.ResidentId == resident.Id)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            return View(bills);
        }

        // My Complaints
        public async Task<IActionResult> MyComplaints()
        {
            if (!IsResident()) return RedirectToAction("Login", "Account");
            var resident = await _context.Residents.FirstOrDefaultAsync(r => r.UserId == GetUserId());
            if (resident == null) return RedirectToAction("Login", "Account");

            var complaints = await _context.Complaints
                .Where(c => c.ResidentId == resident.Id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            ViewBag.ResidentId = resident.Id;
            return View(complaints);
        }

        // POST: Add Complaint
        [HttpPost]
        public async Task<IActionResult> AddComplaint(int residentId, string title, string description)
        {
            if (!IsResident()) return RedirectToAction("Login", "Account");
            var complaint = new Complaint
            {
                ResidentId = residentId,
                Title = title,
                Description = description,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };
            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();
            return RedirectToAction("MyComplaints");
        }

        // My Notices
        public async Task<IActionResult> MyNotices()
        {
            if (!IsResident()) return RedirectToAction("Login", "Account");
            var notices = await _context.Notices.OrderByDescending(n => n.CreatedAt).ToListAsync();
            return View(notices);
        }
        public async Task<IActionResult> Profile()
{
    if (!IsResident()) return RedirectToAction("Login", "Account");
    var resident = await _context.Residents.FirstOrDefaultAsync(r => r.UserId == GetUserId());
    if (resident == null) return RedirectToAction("Login", "Account");
    return View(resident);
}

[HttpPost]
public async Task<IActionResult> Profile(int id, string name, string phone, string address)
{
    if (!IsResident()) return RedirectToAction("Login", "Account");
    var resident = await _context.Residents.FindAsync(id);
    if (resident != null)
    {
        resident.Name = name;
        resident.Phone = phone;
        resident.Address = address;
        await _context.SaveChangesAsync();

        // Update session name
        HttpContext.Session.SetString("UserName", name);
        TempData["Success"] = "Profile updated successfully!";
    }
    return RedirectToAction("Profile");
}
    }
}