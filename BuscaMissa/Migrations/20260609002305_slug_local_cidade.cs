using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuscaMissa.Migrations
{
    /// <inheritdoc />
    public partial class slug_local_cidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Igrejas",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Uf",
                table: "Enderecos",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CidadeSlug",
                table: "Enderecos",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Igrejas_Slug",
                table: "Igrejas",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "IX_Enderecos_Uf_CidadeSlug",
                table: "Enderecos",
                columns: new[] { "Uf", "CidadeSlug" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Igrejas_Slug",
                table: "Igrejas");

            migrationBuilder.DropIndex(
                name: "IX_Enderecos_Uf_CidadeSlug",
                table: "Enderecos");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Igrejas");

            migrationBuilder.DropColumn(
                name: "CidadeSlug",
                table: "Enderecos");

            migrationBuilder.AlterColumn<string>(
                name: "Uf",
                table: "Enderecos",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
