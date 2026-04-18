using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementSystem.Models
{
    public class Complaint
    {
        public int Id { get; set; }

        [Required]
        public int ResidentId { get; set; }

        [ForeignKey("ResidentId")]
        public Resident? Resident { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        // "Pending", "In Progress", "Resolved"
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ResolvedAt { get; set; }
    }
}