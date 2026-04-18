using Microsoft.AspNetCore.Mvc;
using SocietyManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace SocietyManagementSystem.Controllers
{
    public class BillController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BillController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("UserRole") == "Admin";

       public async Task<IActionResult> Index(string? month)
{
    if (!IsAdmin()) return RedirectToAction("Login", "Account");
    var query = _context.MaintenanceBills.Include(b => b.Resident).AsQueryable();

    if (!string.IsNullOrEmpty(month))
        query = query.Where(b => b.Month == month);

    var bills = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
    var months = await _context.MaintenanceBills.Select(b => b.Month).Distinct().ToListAsync();

    ViewBag.Months = months;
    ViewBag.SelectedMonth = month;
    ViewBag.PaidCount = bills.Count(b => b.IsPaid);
    ViewBag.UnpaidCount = bills.Count(b => !b.IsPaid);

    return View(bills);
}

        public async Task<IActionResult> Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            ViewBag.Residents = await _context.Residents.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(MaintenanceBill bill)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            bill.IsPaid = false;
            bill.PaymentStatus = "Unpaid";
            bill.CreatedAt = DateTime.Now;
            _context.MaintenanceBills.Add(bill);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var bill = await _context.MaintenanceBills.FindAsync(id);
            if (bill != null) { bill.IsPaid = true; bill.PaymentStatus = "Paid"; bill.PaidOn = DateTime.Now; await _context.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ApprovePayment(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var bill = await _context.MaintenanceBills.FindAsync(id);
            if (bill != null) { bill.IsPaid = true; bill.PaymentStatus = "Paid"; bill.PaidOn = DateTime.Now; await _context.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var bill = await _context.MaintenanceBills.FindAsync(id);
            if (bill != null) { _context.MaintenanceBills.Remove(bill); await _context.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> PayQR(int id)
        {
            var bill = await _context.MaintenanceBills.Include(b => b.Resident).FirstOrDefaultAsync(b => b.Id == id);
            if (bill == null) return NotFound();

            string upiId = "rushitkhokhani6494@okhdfcbank";
            string upiUrl = $"upi://pay?pa={upiId}&pn=Society+Management&am={bill.Amount}&cu=INR&tn=Maintenance+Bill+{bill.Month}";

            var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(upiUrl, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            byte[] qrBytes = qrCode.GetGraphic(10);
            string qrBase64 = Convert.ToBase64String(qrBytes);

            ViewBag.QRCode = qrBase64;
            ViewBag.Bill = bill;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadScreenshot(int billId, IFormFile screenshot)
        {
            var bill = await _context.MaintenanceBills.FindAsync(billId);
            if (bill != null && screenshot != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = $"payment_{billId}_{DateTime.Now.Ticks}{Path.GetExtension(screenshot.FileName)}";
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await screenshot.CopyToAsync(stream);

                bill.PaymentScreenshot = fileName;
                bill.PaymentStatus = "Pending Verification";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("MyBills", "ResidentPanel");
        }
        // GET: Bulk Bill Create
public async Task<IActionResult> BulkCreate()
{
    if (!IsAdmin()) return RedirectToAction("Login", "Account");
    return View();
}

// POST: Bulk Bill Create
[HttpPost]
public async Task<IActionResult> BulkCreate(string month, decimal amount)
{
    if (!IsAdmin()) return RedirectToAction("Login", "Account");

    var residents = await _context.Residents.ToListAsync();
    int generated = 0;
    int skipped = 0;

    foreach (var resident in residents)
    {
        // Already bill che to skip karo
        var existing = await _context.MaintenanceBills
            .FirstOrDefaultAsync(b => b.ResidentId == resident.Id && b.Month == month);

        if (existing != null) { skipped++; continue; }

        var bill = new MaintenanceBill
        {
            ResidentId = resident.Id,
            Amount = amount,
            Month = month,
            IsPaid = false,
            PaymentStatus = "Unpaid",
            CreatedAt = DateTime.Now
        };
        _context.MaintenanceBills.Add(bill);
        generated++;
    }

    await _context.SaveChangesAsync();
    TempData["Success"] = $"✅ {generated} residents ne bill generate thayu! {skipped} already hata etle skip kaya.";
    return RedirectToAction("Index");
}
        public async Task<IActionResult> DownloadReceipt(int id)
{
    var bill = await _context.MaintenanceBills
        .Include(b => b.Resident)
        .FirstOrDefaultAsync(b => b.Id == id);

    if (bill == null || !bill.IsPaid) return NotFound();

    using var ms = new MemoryStream();
    var writer = new PdfWriter(ms);
    var pdf = new PdfDocument(writer);
    var document = new Document(pdf);

    var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
    var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

    // Header
    document.Add(new Paragraph("SOCIETY MANAGEMENT SYSTEM")
        .SetFont(boldFont).SetFontSize(18)
        .SetTextAlignment(TextAlignment.CENTER));
    document.Add(new Paragraph("PAYMENT RECEIPT")
        .SetFont(boldFont).SetFontSize(14)
        .SetTextAlignment(TextAlignment.CENTER)
        .SetFontColor(new DeviceRgb(0, 114, 255)));
    document.Add(new Paragraph("─────────────────────────────────────────")
        .SetFont(normalFont).SetFontSize(10)
        .SetTextAlignment(TextAlignment.CENTER));
    document.Add(new Paragraph("\n"));

    // Details Table
    var table = new Table(new float[] { 2, 3 });
    table.SetWidth(UnitValue.CreatePercentValue(100));

    void AddRow(string label, string value)
    {
        table.AddCell(new Cell().Add(new Paragraph(label).SetFont(boldFont).SetFontSize(11))
            .SetBackgroundColor(new DeviceRgb(15, 32, 39)).SetFontColor(ColorConstants.WHITE).SetPadding(10));
        table.AddCell(new Cell().Add(new Paragraph(value).SetFont(normalFont).SetFontSize(11))
            .SetBackgroundColor(new DeviceRgb(26, 39, 68)).SetFontColor(ColorConstants.WHITE).SetPadding(10));
    }

    AddRow("Receipt No", $"RCP-{bill.Id:D5}");
    AddRow("Resident Name", bill.Resident?.Name ?? "-");
    AddRow("Flat Number", bill.Resident?.FlatNumber ?? "-");
    AddRow("Month", bill.Month);
    AddRow("Amount Paid", $"Rs. {bill.Amount.ToString("0.##")}");
    AddRow("Payment Date", bill.PaidOn?.ToString("dd MMM yyyy") ?? "-");
    AddRow("Payment Status", "✅ PAID");

    document.Add(table);
    document.Add(new Paragraph("\n\n"));
    document.Add(new Paragraph("Thank you for your payment!")
        .SetFont(boldFont).SetFontSize(12)
        .SetTextAlignment(TextAlignment.CENTER)
        .SetFontColor(new DeviceRgb(0, 198, 255)));
    document.Add(new Paragraph("This is a computer generated receipt.")
        .SetFont(normalFont).SetFontSize(10)
        .SetTextAlignment(TextAlignment.CENTER)
        .SetFontColor(new DeviceRgb(128, 128, 128)));

    document.Close();
    return File(ms.ToArray(), "application/pdf", $"Receipt_{bill.Resident?.FlatNumber}_{bill.Month}.pdf");
}
        public async Task<IActionResult> DownloadReport()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var bills = await _context.MaintenanceBills
                .Include(b => b.Resident)
                .OrderBy(b => b.Resident!.FlatNumber)
                .ToListAsync();

            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            document.Add(new Paragraph("Society Management System")
                .SetFont(boldFont).SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("Maintenance Bills Report - " + DateTime.Now.ToString("dd MMM yyyy"))
                .SetFont(normalFont).SetFontSize(11)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("\n"));

            var table = new Table(new float[] { 1, 2, 3, 2, 2, 2, 2 });
            table.SetWidth(UnitValue.CreatePercentValue(100));

            string[] headers = { "#", "Flat No", "Resident", "Month", "Amount", "Status", "Paid On" };
            foreach (var h in headers)
            {
                table.AddHeaderCell(new Cell().Add(new Paragraph(h).SetFont(boldFont).SetFontSize(10))
                    .SetBackgroundColor(new DeviceRgb(233, 69, 96))
                    .SetFontColor(ColorConstants.WHITE));
            }

            int sr = 1;
            int totalPaid = 0, totalUnpaid = 0, totalPending = 0;

            foreach (var b in bills)
            {
                string status;
                if (b.IsPaid) { status = "Paid"; totalPaid++; }
                else if (b.PaymentStatus == "Pending Verification") { status = "Pending"; totalPending++; }
                else { status = "Unpaid"; totalUnpaid++; }

                table.AddCell(new Cell().Add(new Paragraph(sr++.ToString()).SetFont(normalFont).SetFontSize(9)));
                table.AddCell(new Cell().Add(new Paragraph(b.Resident?.FlatNumber ?? "-").SetFont(boldFont).SetFontSize(9)));
                table.AddCell(new Cell().Add(new Paragraph(b.Resident?.Name ?? "-").SetFont(normalFont).SetFontSize(9)));
                table.AddCell(new Cell().Add(new Paragraph(b.Month).SetFont(normalFont).SetFontSize(9)));
                table.AddCell(new Cell().Add(new Paragraph("Rs. " + b.Amount.ToString("0.##")).SetFont(normalFont).SetFontSize(9)));

                var statusPara = new Paragraph(status).SetFont(boldFont).SetFontSize(9);
                if (b.IsPaid) statusPara.SetFontColor(new DeviceRgb(46, 204, 113));
                else if (status == "Pending") statusPara.SetFontColor(new DeviceRgb(241, 196, 15));
                else statusPara.SetFontColor(new DeviceRgb(233, 69, 96));
                table.AddCell(new Cell().Add(statusPara));

                table.AddCell(new Cell().Add(new Paragraph(b.PaidOn.HasValue ? b.PaidOn.Value.ToString("dd MMM yyyy") : "-").SetFont(normalFont).SetFontSize(9)));
            }

            document.Add(table);
            document.Add(new Paragraph("\n"));
            document.Add(new Paragraph($"Total: {bills.Count}  |  Paid: {totalPaid}  |  Pending: {totalPending}  |  Unpaid: {totalUnpaid}")
                .SetFont(boldFont).SetFontSize(11));

            document.Close();
            return File(ms.ToArray(), "application/pdf", $"BillsReport_{DateTime.Now:yyyyMMdd}.pdf");
        }
    }
}