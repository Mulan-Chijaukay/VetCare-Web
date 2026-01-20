using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCare.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPrioridadCita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Prioridad",
                table: "Citas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Prioridad",
                table: "Citas");
        }
    }
}
