using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuscaMissa.Migrations
{
    /// <inheritdoc />
    public partial class criar_metricas_diarias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MetricasDiarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TipoEntidade = table.Column<int>(type: "int", nullable: false),
                    EntidadeId = table.Column<int>(type: "int", nullable: false),
                    TipoMetrica = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricasDiarias", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MetricasDiarias_Data",
                table: "MetricasDiarias",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_MetricasDiarias_TipoEntidade_EntidadeId",
                table: "MetricasDiarias",
                columns: new[] { "TipoEntidade", "EntidadeId" });

            migrationBuilder.CreateIndex(
                name: "IX_MetricasDiarias_TipoEntidade_EntidadeId_TipoMetrica_Data",
                table: "MetricasDiarias",
                columns: new[] { "TipoEntidade", "EntidadeId", "TipoMetrica", "Data" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetricasDiarias");
        }
    }
}
