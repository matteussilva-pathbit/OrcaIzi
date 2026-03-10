using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrcaIzi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetPaymentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "Budgets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentCreatedAt",
                table: "Budgets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentExternalId",
                table: "Budgets",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentLink",
                table: "Budgets",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentProvider",
                table: "Budgets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentQrCode",
                table: "Budgets",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentQrCodeBase64",
                table: "Budgets",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Budgets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "PaymentCreatedAt",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "PaymentExternalId",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "PaymentLink",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "PaymentProvider",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "PaymentQrCode",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "PaymentQrCodeBase64",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Budgets");
        }
    }
}
