using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocietyManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentScreenshot",
                table: "MaintenanceBills",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "MaintenanceBills",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentScreenshot",
                table: "MaintenanceBills");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "MaintenanceBills");
        }
    }
}
