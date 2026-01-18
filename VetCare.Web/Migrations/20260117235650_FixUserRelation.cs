using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCare.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixUserRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DuenoId",
                table: "Mascotas",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mascotas_DuenoId",
                table: "Mascotas",
                column: "DuenoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Mascotas_AspNetUsers_DuenoId",
                table: "Mascotas",
                column: "DuenoId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Mascotas_AspNetUsers_DuenoId",
                table: "Mascotas");

            migrationBuilder.DropIndex(
                name: "IX_Mascotas_DuenoId",
                table: "Mascotas");

            migrationBuilder.DropColumn(
                name: "DuenoId",
                table: "Mascotas");
        }
    }
}
