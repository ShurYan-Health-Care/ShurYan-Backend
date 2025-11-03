using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuryan.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddActualSessionTimesToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActualEndTime",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualStartTime",
                table: "Appointments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualEndTime",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ActualStartTime",
                table: "Appointments");
        }
    }
}
