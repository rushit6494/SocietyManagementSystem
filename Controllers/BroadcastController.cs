using Microsoft.AspNetCore.Mvc;
using SocietyManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using MailKit.Net.Smtp;
using MimeKit;

namespace SocietyManagementSystem.Controllers
{
    public class BroadcastController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public BroadcastController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("UserRole") == "Admin";

        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Send(string subject, string message)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var residents = await _context.Residents.ToListAsync();
            int sent = 0;

            var smtpServer = _config["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"]!);
            var senderEmail = _config["EmailSettings:SenderEmail"];
            var senderPassword = _config["EmailSettings:SenderPassword"];
            var senderName = _config["EmailSettings:SenderName"];

            foreach (var resident in residents)
            {
                if (string.IsNullOrEmpty(resident.Email)) continue;
                try
                {
                    var email = new MimeMessage();
                    email.From.Add(new MailboxAddress(senderName, senderEmail));
                    email.To.Add(new MailboxAddress(resident.Name, resident.Email));
                    email.Subject = subject;

                    var bodyBuilder = new BodyBuilder();
                    bodyBuilder.HtmlBody = $@"
                    <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#0a0e1a;padding:20px;border-radius:10px;'>
                        <div style='background:linear-gradient(135deg,#00c6ff,#0072ff);padding:20px;border-radius:10px 10px 0 0;text-align:center;'>
                            <h2 style='color:#fff;margin:0;'>📢 Society Broadcast</h2>
                            <p style='color:rgba(255,255,255,0.8);margin:5px 0 0;'>Society Management System</p>
                        </div>
                        <div style='background:#111827;padding:25px;border-radius:0 0 10px 10px;border:1px solid rgba(0,198,255,0.1);'>
                            <h3 style='color:#00c6ff;'>{subject}</h3>
                            <div style='background:#0f2027;padding:15px;border-radius:8px;border-left:4px solid #00c6ff;'>
                                <p style='color:#fff;margin:0;line-height:1.7;'>{message}</p>
                            </div>
                            <hr style='border:none;border-top:1px solid rgba(255,255,255,0.1);margin:20px 0;'/>
                            <p style='color:rgba(255,255,255,0.4);font-size:12px;text-align:center;'>
                                Dear {resident.Name} | Flat: {resident.FlatNumber}<br/>
                                Society Management System
                            </p>
                        </div>
                    </div>";

                    email.Body = bodyBuilder.ToMessageBody();

                    using var client = new SmtpClient();
                    await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(senderEmail, senderPassword);
                    await client.SendAsync(email);
                    await client.DisconnectAsync(true);
                    sent++;
                }
                catch { }
            }

            TempData["Success"] = $"✅ {sent} residents ne message mokali devayo!";
            return RedirectToAction("Index");
        }
    }
}