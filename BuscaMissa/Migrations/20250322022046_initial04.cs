using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuscaMissa.Migrations
{
    /// <inheritdoc />
    public partial class initial04 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Bloqueado",
                table: "Usuarios",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoBloqueio",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bloqueado",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "MotivoBloqueio",
                table: "Usuarios");
        }
    }
}
