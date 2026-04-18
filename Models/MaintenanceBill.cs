using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementSystem.Models
{
    public class MaintenanceBill
    {
        public int Id { get; set; }

        [Required]
        public int ResidentId { get; set; }

        [ForeignKey("ResidentId")]
        public Resident? Resident { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Month { get; set; } = string.Empty; // e.g. "March 2026"

        public bool IsPaid { get; set; } = false;

        public DateTime? PaidOn { get; set; }

        public string? PaymentScreenshot { get; set; }

        // "Unpaid", "Pending Verification", "Paid"
        public string PaymentStatus { get; set; } = "Unpaid";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
} 