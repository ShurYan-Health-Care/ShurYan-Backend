using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuryan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Telemedicine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TelemedicineSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    DoctorConnectionId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PatientConnectionId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DoctorJoinedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PatientJoinedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    EndReason = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemedicineSessions", x => x.Id);
                    table.CheckConstraint("CK_TelemedicineSession_Duration", "[DurationSeconds] IS NULL OR [DurationSeconds] >= 0");
                    table.ForeignKey(
                        name: "FK_TelemedicineSessions_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TelemedicineSessions_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TelemedicineSessions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TelemedicineSession_AppointmentId",
                table: "TelemedicineSessions",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelemedicineSession_DoctorId",
                table: "TelemedicineSessions",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TelemedicineSession_PatientId",
                table: "TelemedicineSessions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TelemedicineSession_RoomId",
                table: "TelemedicineSessions",
                column: "RoomId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelemedicineSession_Status",
                table: "TelemedicineSessions",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TelemedicineSessions");
        }
    }
}
