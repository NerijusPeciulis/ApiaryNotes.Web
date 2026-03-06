using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiaryNotes.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductTypeToHiveHarvest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Product",
                table: "HiveHarvests",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Product",
                table: "HiveHarvests");
        }
    }
}
