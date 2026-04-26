using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuryan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencySosEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BloodType",
                table: "Patients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactRelationship",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmergencyModeActivatedById",
                table: "Patients",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmergencyModeActive",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "HeightCm",
                table: "Patients",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPregnant",
                table: "Patients",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhysicalDisabilities",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightKg",
                table: "Patients",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dosage",
                table: "MedicalHistoryItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Frequency",
                table: "MedicalHistoryItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReactionType",
                table: "MedicalHistoryItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmergencyAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmergencyAuditLogs_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmergencyAuditLogs_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmergencyEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActivatingDoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    MedicalRecordSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmergencyEvents_Doctors_ActivatingDoctorId",
                        column: x => x.ActivatingDoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmergencyEvents_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_EmergencyModeActivatedById",
                table: "Patients",
                column: "EmergencyModeActivatedById");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAuditLogs_DoctorId",
                table: "EmergencyAuditLogs",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAuditLogs_PatientId",
                table: "EmergencyAuditLogs",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyEvents_ActivatingDoctorId",
                table: "EmergencyEvents",
                column: "ActivatingDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyEvents_PatientId",
                table: "EmergencyEvents",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Doctors_EmergencyModeActivatedById",
                table: "Patients",
                column: "EmergencyModeActivatedById",
                principalTable: "Doctors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Doctors_EmergencyModeActivatedById",
                table: "Patients");

            migrationBuilder.DropTable(
                name: "EmergencyAuditLogs");

            migrationBuilder.DropTable(
                name: "EmergencyEvents");

            migrationBuilder.DropIndex(
                name: "IX_Patients_EmergencyModeActivatedById",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "BloodType",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "EmergencyContactRelationship",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "EmergencyModeActivatedById",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "EmergencyModeActive",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "HeightCm",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "IsPregnant",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "PhysicalDisabilities",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "WeightKg",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Dosage",
                table: "MedicalHistoryItems");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "MedicalHistoryItems");

            migrationBuilder.DropColumn(
                name: "ReactionType",
                table: "MedicalHistoryItems");
        }
    }
}
