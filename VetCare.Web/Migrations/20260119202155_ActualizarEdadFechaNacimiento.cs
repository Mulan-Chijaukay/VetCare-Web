using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VetCare.Web.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarEdadFechaNacimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "Edad",
                table: "Mascotas");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNacimiento",
                table: "Mascotas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                table: "Mascotas");

            migrationBuilder.AddColumn<string>(
                name: "DuenoId",
                table: "Mascotas",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Edad",
                table: "Mascotas",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
    }
}
