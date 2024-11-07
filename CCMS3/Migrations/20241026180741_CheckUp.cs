using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CCMS3.Migrations
{
    /// <inheritdoc />
    public partial class CheckUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonalDetails_EmploymentStatus_EmploymentStatusId",
                table: "PersonalDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmploymentStatus",
                table: "EmploymentStatus");

            migrationBuilder.RenameTable(
                name: "EmploymentStatus",
                newName: "EmploymentStatuses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmploymentStatuses",
                table: "EmploymentStatuses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonalDetails_EmploymentStatuses_EmploymentStatusId",
                table: "PersonalDetails",
                column: "EmploymentStatusId",
                principalTable: "EmploymentStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonalDetails_EmploymentStatuses_EmploymentStatusId",
                table: "PersonalDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmploymentStatuses",
                table: "EmploymentStatuses");

            migrationBuilder.RenameTable(
                name: "EmploymentStatuses",
                newName: "EmploymentStatus");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmploymentStatus",
                table: "EmploymentStatus",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonalDetails_EmploymentStatus_EmploymentStatusId",
                table: "PersonalDetails",
                column: "EmploymentStatusId",
                principalTable: "EmploymentStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
