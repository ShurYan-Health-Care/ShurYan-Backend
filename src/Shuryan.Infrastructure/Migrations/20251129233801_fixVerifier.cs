using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shuryan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixVerifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Verifier_CreatedByAdminId",
                table: "Verifiers");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedByAdminId",
                table: "Verifiers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedByAdminId",
                table: "Verifiers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Verifier_CreatedByAdminId",
                table: "Verifiers",
                column: "CreatedByAdminId");
        }
    }
}
