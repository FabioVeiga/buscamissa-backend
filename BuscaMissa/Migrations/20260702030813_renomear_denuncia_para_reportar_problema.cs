using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuscaMissa.Migrations
{
    /// <inheritdoc />
    public partial class renomear_denuncia_para_reportar_problema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "IgrejaDenuncias",
                newName: "IgrejaReportarProblemas");

            migrationBuilder.RenameColumn(
                name: "NomeDenunciador",
                table: "IgrejaReportarProblemas",
                newName: "Nome");

            migrationBuilder.RenameColumn(
                name: "EmailDenunciador",
                table: "IgrejaReportarProblemas",
                newName: "Email");

            migrationBuilder.RenameIndex(
                name: "IX_IgrejaDenuncias_IgrejaId",
                table: "IgrejaReportarProblemas",
                newName: "IX_IgrejaReportarProblemas_IgrejaId");

            migrationBuilder.DropColumn(
                name: "Titulo",
                table: "IgrejaReportarProblemas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Titulo",
                table: "IgrejaReportarProblemas",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.RenameIndex(
                name: "IX_IgrejaReportarProblemas_IgrejaId",
                table: "IgrejaReportarProblemas",
                newName: "IX_IgrejaDenuncias_IgrejaId");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "IgrejaReportarProblemas",
                newName: "EmailDenunciador");

            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "IgrejaReportarProblemas",
                newName: "NomeDenunciador");

            migrationBuilder.RenameTable(
                name: "IgrejaReportarProblemas",
                newName: "IgrejaDenuncias");
        }
    }
}
