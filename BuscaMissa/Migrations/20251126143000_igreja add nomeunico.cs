using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuscaMissa.Migrations
{
    /// <inheritdoc />
    public partial class igrejaaddnomeunico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NomeUnico",
                table: "Igrejas",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NomeUnico",
                table: "Igrejas");
        }
    }
}
