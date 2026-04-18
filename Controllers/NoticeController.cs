using Microsoft.AspNetCore.Mvc;
using SocietyManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using MailKit.Net.Smtp;
using MimeKit;

namespace SocietyManagementSystem.Controllers
{
    public class NoticeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public NoticeController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("UserRole") == "Admin";

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var notices = await _context.Notices.OrderByDescending(n => n.CreatedAt).ToListAsync();
            return View(notices);
        }

        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Notice notice)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            notice.CreatedAt = DateTime.Now;
            _context.Notices.Add(notice);
            await _context.SaveChangesAsync();

            // Send email to all residents
            await SendNoticeEmailToAllResidents(notice);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var notice = await _context.Notices.FindAsync(id);
            if (notice != null) { _context.Notices.Remove(notice); await _context.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        private async Task SendNoticeEmailToAllResidents(Notice notice)
        {
            try
            {
                var residents = await _context.Residents.ToListAsync();
                var smtpServer = _config["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"]!);
                var senderEmail = _config["EmailSettings:SenderEmail"];
                var senderPassword = _config["EmailSettings:SenderPassword"];
                var senderName = _config["EmailSettings:SenderName"];

                foreach (var resident in residents)
                {
                    if (string.IsNullOrEmpty(resident.Email)) continue;

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(senderName, senderEmail));
                    message.To.Add(new MailboxAddress(resident.Name, resident.Email));
                    message.Subject = $"📢 New Notice: {notice.Title}";

                    var bodyBuilder = new BodyBuilder();
                    bodyBuilder.HtmlBody = $@"
                    <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#f9f9f9;padding:20px;border-radius:10px;'>
                        <div style='background:linear-gradient(135deg,#e94560,#c62a47);padding:20px;border-radius:10px 10px 0 0;text-align:center;'>
                            <h2 style='color:#fff;margin:0;'>📢 Society Notice</h2>
                            <p style='color:rgba(255,255,255,0.8);margin:5px 0 0;'>Society Management System</p>
                        </div>
                        <div style='background:#fff;padding:25px;border-radius:0 0 10px 10px;'>
                            <h3 style='color:#333;'>{notice.Title}</h3>
                            <div style='background:#f5f5f5;padding:15px;border-radius:8px;border-left:4px solid #e94560;'>
                                <p style='color:#555;margin:0;line-height:1.6;'>{notice.Content}</p>
                            </div>
                            <div style='margin-top:15px;'>
                                <span style='background:{(notice.Category == "Urgent" ? "#e94560" : notice.Category == "Event" ? "#3498db" : "#2ecc71")};color:#fff;padding:5px 12px;border-radius:20px;font-size:12px;'>{notice.Category}</span>
                                <span style='color:#999;font-size:12px;margin-left:10px;'>📅 {notice.CreatedAt.ToString("dd MMM yyyy")}</span>
                            </div>
                            <hr style='border:none;border-top:1px solid #eee;margin:20px 0;'/>
                            <p style='color:#999;font-size:12px;text-align:center;'>
                                This is an automated notification from Society Management System.<br/>
                                Dear {resident.Name} | Flat: {resident.FlatNumber}
                            </p>
                        </div>
                    </div>";

                    message.Body = bodyBuilder.ToMessageBody();

                    using var client = new SmtpClient();
                    await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(senderEmail, senderPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email error: {ex.Message}");
            }
        }
    }
}