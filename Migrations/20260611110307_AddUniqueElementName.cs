using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace genshin.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueElementName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Elements_Name",
                table: "Elements",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Elements_Name",
                table: "Elements");
        }
    }
}
