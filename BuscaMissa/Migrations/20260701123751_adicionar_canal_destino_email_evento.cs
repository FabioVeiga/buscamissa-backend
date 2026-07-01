using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuscaMissa.Migrations
{
    /// <inheritdoc />
    public partial class adicionar_canal_destino_email_evento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Canal",
                table: "EmailEventosIgreja",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "DestinoContato",
                table: "EmailEventosIgreja",
                type: "varchar(300)",
                maxLength: 300,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Canal",
                table: "EmailEventosIgreja");

            migrationBuilder.DropColumn(
                name: "DestinoContato",
                table: "EmailEventosIgreja");
        }
    }
}
