using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCare.Web.Migrations
{
    /// <inheritdoc />
    public partial class HistorialClinicoforeigkcita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_HistoriasClinicas_CitaId",
                table: "HistoriasClinicas",
                column: "CitaId");

            migrationBuilder.AddForeignKey(
                name: "FK_HistoriasClinicas_Citas_CitaId",
                table: "HistoriasClinicas",
                column: "CitaId",
                principalTable: "Citas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HistoriasClinicas_Citas_CitaId",
                table: "HistoriasClinicas");

            migrationBuilder.DropIndex(
                name: "IX_HistoriasClinicas_CitaId",
                table: "HistoriasClinicas");
        }
    }
}
