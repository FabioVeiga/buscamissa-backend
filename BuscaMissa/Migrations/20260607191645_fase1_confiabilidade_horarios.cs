using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuscaMissa.Migrations
{
    /// <inheritdoc />
    public partial class fase1_confiabilidade_horarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfirmacoesHorario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IgrejaId = table.Column<int>(type: "int", nullable: false),
                    HashFingerprint = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnderecoIp = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfirmacoesHorario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfirmacoesHorario_Igrejas_IgrejaId",
                        column: x => x.IgrejaId,
                        principalTable: "Igrejas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReportesHorario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IgrejaId = table.Column<int>(type: "int", nullable: false),
                    Motivos = table.Column<int>(type: "int", nullable: false),
                    Descricao = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FonteInformacao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HashFingerprint = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnderecoIp = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportesHorario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportesHorario_Igrejas_IgrejaId",
                        column: x => x.IgrejaId,
                        principalTable: "Igrejas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmacoesHorario_DataCriacao",
                table: "ConfirmacoesHorario",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmacoesHorario_IgrejaId_HashFingerprint",
                table: "ConfirmacoesHorario",
                columns: new[] { "IgrejaId", "HashFingerprint" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportesHorario_DataCriacao",
                table: "ReportesHorario",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesHorario_IgrejaId_Status",
                table: "ReportesHorario",
                columns: new[] { "IgrejaId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfirmacoesHorario");

            migrationBuilder.DropTable(
                name: "ReportesHorario");
        }
    }
}
