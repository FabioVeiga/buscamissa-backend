using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuscaMissa.Migrations
{
    /// <inheritdoc />
    public partial class initial02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "Site",
                table: "RedesSociais");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "RedesSociais");

            migrationBuilder.AlterColumn<int>(
                name: "TipoRedeSocial",
                table: "RedesSociais",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeDoPerfil",
                table: "RedesSociais",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "Verificado",
                table: "RedesSociais",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "NomeDoPerfil",
                table: "RedesSociais");

            migrationBuilder.DropColumn(
                name: "Verificado",
                table: "RedesSociais");

            migrationBuilder.AlterColumn<int>(
                name: "TipoRedeSocial",
                table: "RedesSociais",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Site",
                table: "RedesSociais",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "RedesSociais",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

        }
    }
}
