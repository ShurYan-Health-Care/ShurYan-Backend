using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuryan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDoctorAppointmentDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_DoctorConsultation_Duration",
                table: "DoctorConsultations");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DoctorConsultation_Duration",
                table: "DoctorConsultations",
                sql: "[SessionDurationMinutes] >= 5 AND [SessionDurationMinutes] <= 480");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_DoctorConsultation_Duration",
                table: "DoctorConsultations");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DoctorConsultation_Duration",
                table: "DoctorConsultations",
                sql: "[SessionDurationMinutes] >= 15 AND [SessionDurationMinutes] <= 120");
        }
    }
}
