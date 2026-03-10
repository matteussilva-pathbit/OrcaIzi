using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrcaIzi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetPublicShare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PublicShareCreatedAt",
                table: "Budgets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PublicShareEnabled",
                table: "Budgets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PublicShareId",
                table: "Budgets",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicShareCreatedAt",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "PublicShareEnabled",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "PublicShareId",
                table: "Budgets");
        }
    }
}
