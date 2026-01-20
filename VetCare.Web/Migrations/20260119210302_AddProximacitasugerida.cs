using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCare.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddProximacitasugerida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ProximaCitaSugerida",
                table: "HistoriasClinicas",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProximaCitaSugerida",
                table: "HistoriasClinicas");
        }
    }
}
