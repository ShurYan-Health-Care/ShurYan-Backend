using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuryan.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryFeeToPharmacy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryFee",
                table: "Pharmacies",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Appointment_Doctor_Status_StartTime",
                table: "Appointments",
                columns: new[] { "DoctorId", "Status", "ScheduledStartTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointment_Doctor_Status_StartTime",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "DeliveryFee",
                table: "Pharmacies");
        }
    }
}
