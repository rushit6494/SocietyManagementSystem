using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementSystem.Models
{
    public class Visitor
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        public string Purpose { get; set; } = string.Empty;

        [Required]
        public int ResidentId { get; set; }

        [ForeignKey("ResidentId")]
        public Resident? Resident { get; set; }

        public DateTime VisitDate { get; set; } = DateTime.Now;

        // "Inside", "Left"
        public string Status { get; set; } = "Inside";
    }
}