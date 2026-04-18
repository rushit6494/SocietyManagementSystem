using System.ComponentModel.DataAnnotations;

namespace SocietyManagementSystem.Models
{
    public class Notice
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        // "General", "Urgent", "Event"
        public string Category { get; set; } = "General";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ExpiryDate { get; set; }
    }
}