using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace genshin.Migrations
{
    /// <inheritdoc />
    public partial class ele : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_Element_ElementId",
                table: "Characters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Element",
                table: "Element");

            migrationBuilder.RenameTable(
                name: "Element",
                newName: "Elements");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Elements",
                table: "Elements",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_Elements_ElementId",
                table: "Characters",
                column: "ElementId",
                principalTable: "Elements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_Elements_ElementId",
                table: "Characters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Elements",
                table: "Elements");

            migrationBuilder.RenameTable(
                name: "Elements",
                newName: "Element");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Element",
                table: "Element",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_Element_ElementId",
                table: "Characters",
                column: "ElementId",
                principalTable: "Element",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
