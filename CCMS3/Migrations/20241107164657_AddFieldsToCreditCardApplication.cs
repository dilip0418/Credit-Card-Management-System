using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CCMS3.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsToCreditCardApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNo",
                table: "CreditCardApplications",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualIncome",
                table: "CreditCardApplications",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "CreditCardApplications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnnualIncome",
                table: "CreditCardApplications");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "CreditCardApplications");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNo",
                table: "CreditCardApplications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);
        }
    }
}
