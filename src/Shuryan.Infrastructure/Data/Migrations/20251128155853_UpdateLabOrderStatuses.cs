using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuryan.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLabOrderStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "LabOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "LabOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "LabOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SamplesCollectedAt",
                table: "LabOrders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "LabOrders");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "LabOrders");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "LabOrders");

            migrationBuilder.DropColumn(
                name: "SamplesCollectedAt",
                table: "LabOrders");
        }
    }
}
