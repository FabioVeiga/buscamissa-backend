using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuscaMissa.Migrations
{
    /// <inheritdoc />
    public partial class initial06 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IgrejaDenuncias_IgrejaId",
                table: "IgrejaDenuncias");

            migrationBuilder.AddColumn<string>(
                name: "Latitude",
                table: "Enderecos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Longitude",
                table: "Enderecos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Contribuidores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataContribuicao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contribuidores", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IgrejaDenuncias_IgrejaId",
                table: "IgrejaDenuncias",
                column: "IgrejaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contribuidores");

            migrationBuilder.DropIndex(
                name: "IX_IgrejaDenuncias_IgrejaId",
                table: "IgrejaDenuncias");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Enderecos");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Enderecos");

            migrationBuilder.CreateIndex(
                name: "IX_IgrejaDenuncias_IgrejaId",
                table: "IgrejaDenuncias",
                column: "IgrejaId");
        }
    }
}
