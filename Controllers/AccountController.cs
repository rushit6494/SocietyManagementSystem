using Microsoft.AspNetCore.Mvc;
using SocietyManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace SocietyManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
            if (user == null) { ViewBag.Error = "Invalid email or password!"; return View(); }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserRole", user.Role);

            return user.Role == "Admin" ? RedirectToAction("Index", "Admin") : RedirectToAction("Dashboard", "ResidentPanel");
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string name, string phone, string flatNumber, string address, string email, string password)
        {
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existing != null) { ViewBag.Error = "Email already registered! Please login."; return View(); }

            var user = new User { Name = name, Email = email, Password = password, Role = "Resident", CreatedAt = DateTime.Now };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var resident = new Resident { Name = name, Phone = phone, FlatNumber = flatNumber, Address = address, Email = email, UserId = user.Id, JoinDate = DateTime.Now };
            _context.Residents.Add(resident);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Account created! Please login.";
            return RedirectToAction("Login");
        }

        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword) { ViewBag.Error = "Passwords do not match!"; return View(); }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) { ViewBag.Error = "Email not found!"; return View(); }

            user.Password = newPassword;
            await _context.SaveChangesAsync();
            ViewBag.Success = "Password reset successfully!";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}