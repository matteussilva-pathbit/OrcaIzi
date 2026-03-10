using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrcaIzi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ForceNullableDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE [BudgetItems] ALTER COLUMN [Description] nvarchar(500) NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
