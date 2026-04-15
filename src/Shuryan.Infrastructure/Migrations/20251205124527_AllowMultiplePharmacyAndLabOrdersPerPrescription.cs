using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuryan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultiplePharmacyAndLabOrdersPerPrescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PharmacyOrders_PrescriptionId",
                table: "PharmacyOrders");

            migrationBuilder.DropIndex(
                name: "IX_LabOrder_LabPrescriptionId",
                table: "LabOrders");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyOrders_PrescriptionId",
                table: "PharmacyOrders",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_LabOrder_LabPrescriptionId",
                table: "LabOrders",
                column: "LabPrescriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PharmacyOrders_PrescriptionId",
                table: "PharmacyOrders");

            migrationBuilder.DropIndex(
                name: "IX_LabOrder_LabPrescriptionId",
                table: "LabOrders");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyOrders_PrescriptionId",
                table: "PharmacyOrders",
                column: "PrescriptionId",
                unique: true,
                filter: "[PrescriptionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LabOrder_LabPrescriptionId",
                table: "LabOrders",
                column: "LabPrescriptionId",
                unique: true);
        }
    }
}
