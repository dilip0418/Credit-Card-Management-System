using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CCMS3.Migrations
{
    /// <inheritdoc />
    public partial class CorrectPersonalDetailsAndEmpStatsRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PersonalDetails_EmploymentStatusId",
                table: "PersonalDetails");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalDetails_EmploymentStatusId",
                table: "PersonalDetails",
                column: "EmploymentStatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PersonalDetails_EmploymentStatusId",
                table: "PersonalDetails");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalDetails_EmploymentStatusId",
                table: "PersonalDetails",
                column: "EmploymentStatusId",
                unique: true);
        }
    }
}
