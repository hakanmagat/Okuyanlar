using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Okuyanlar.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddShelfLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShelfLocation",
                table: "Books",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShelfLocation",
                table: "Books");
        }
    }
}
