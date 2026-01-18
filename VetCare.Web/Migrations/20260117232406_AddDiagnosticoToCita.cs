using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCare.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddDiagnosticoToCita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Diagnostico",
                table: "Citas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Diagnostico",
                table: "Citas");
        }
    }
}
