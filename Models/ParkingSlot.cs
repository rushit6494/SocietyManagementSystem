using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocietyManagementSystem.Models
{
    public class ParkingSlot
    {
        public int Id { get; set; }

        [Required]
        public string SlotNumber { get; set; } = string.Empty;

        // "Car", "Bike", "Other"
        public string VehicleType { get; set; } = "Car";

        public bool IsOccupied { get; set; } = false;

        public int? ResidentId { get; set; }

        [ForeignKey("ResidentId")]
        public Resident? Resident { get; set; }

        public string VehicleNumber { get; set; } = string.Empty;
    }
}