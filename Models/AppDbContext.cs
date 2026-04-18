using Microsoft.EntityFrameworkCore;

namespace SocietyManagementSystem.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Resident> Residents { get; set; }
        public DbSet<MaintenanceBill> MaintenanceBills { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<ParkingSlot> ParkingSlots { get; set; }
        public DbSet<Visitor> Visitors { get; set; }
    }
}