using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuscaMissa.Migrations
{
    /// <inheritdoc />
    public partial class igreja_melhorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AvaliacoesIgreja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IgrejaId = table.Column<int>(type: "int", nullable: false),
                    Nota = table.Column<int>(type: "int", nullable: false),
                    HashFingerprint = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvaliacoesIgreja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvaliacoesIgreja_Igrejas_IgrejaId",
                        column: x => x.IgrejaId,
                        principalTable: "Igrejas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ComentariosIgreja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IgrejaId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Comentario = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HashFingerprint = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnderecoIp = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Aprovado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MotivoBloqueio = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComentariosIgreja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComentariosIgreja_Igrejas_IgrejaId",
                        column: x => x.IgrejaId,
                        principalTable: "Igrejas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CurtidasIgreja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IgrejaId = table.Column<int>(type: "int", nullable: false),
                    HashFingerprint = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnderecoIp = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurtidasIgreja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurtidasIgreja_Igrejas_IgrejaId",
                        column: x => x.IgrejaId,
                        principalTable: "Igrejas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EstatisticasEngajamentoIgreja",
                columns: table => new
                {
                    IgrejaId = table.Column<int>(type: "int", nullable: false),
                    TotalCurtidas = table.Column<int>(type: "int", nullable: false),
                    TotalAvaliacoes = table.Column<int>(type: "int", nullable: false),
                    MediaAvaliacoes = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TotalComentarios = table.Column<int>(type: "int", nullable: false),
                    TotalVisualizacoes = table.Column<int>(type: "int", nullable: false),
                    UltimaAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstatisticasEngajamentoIgreja", x => x.IgrejaId);
                    table.ForeignKey(
                        name: "FK_EstatisticasEngajamentoIgreja_Igrejas_IgrejaId",
                        column: x => x.IgrejaId,
                        principalTable: "Igrejas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VisualizacoesIgreja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IgrejaId = table.Column<int>(type: "int", nullable: false),
                    HashFingerprint = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnderecoIp = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisualizacoesIgreja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisualizacoesIgreja_Igrejas_IgrejaId",
                        column: x => x.IgrejaId,
                        principalTable: "Igrejas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesIgreja_IgrejaId",
                table: "AvaliacoesIgreja",
                column: "IgrejaId");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesIgreja_IgrejaId_HashFingerprint",
                table: "AvaliacoesIgreja",
                columns: new[] { "IgrejaId", "HashFingerprint" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComentariosIgreja_Aprovado",
                table: "ComentariosIgreja",
                column: "Aprovado");

            migrationBuilder.CreateIndex(
                name: "IX_ComentariosIgreja_IgrejaId",
                table: "ComentariosIgreja",
                column: "IgrejaId");

            migrationBuilder.CreateIndex(
                name: "IX_ComentariosIgreja_IgrejaId_Aprovado",
                table: "ComentariosIgreja",
                columns: new[] { "IgrejaId", "Aprovado" });

            migrationBuilder.CreateIndex(
                name: "IX_CurtidasIgreja_EnderecoIp",
                table: "CurtidasIgreja",
                column: "EnderecoIp");

            migrationBuilder.CreateIndex(
                name: "IX_CurtidasIgreja_IgrejaId",
                table: "CurtidasIgreja",
                column: "IgrejaId");

            migrationBuilder.CreateIndex(
                name: "IX_CurtidasIgreja_IgrejaId_HashFingerprint",
                table: "CurtidasIgreja",
                columns: new[] { "IgrejaId", "HashFingerprint" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstatisticasEngajamentoIgreja_MediaAvaliacoes",
                table: "EstatisticasEngajamentoIgreja",
                column: "MediaAvaliacoes");

            migrationBuilder.CreateIndex(
                name: "IX_EstatisticasEngajamentoIgreja_TotalCurtidas",
                table: "EstatisticasEngajamentoIgreja",
                column: "TotalCurtidas");

            migrationBuilder.CreateIndex(
                name: "IX_EstatisticasEngajamentoIgreja_TotalVisualizacoes",
                table: "EstatisticasEngajamentoIgreja",
                column: "TotalVisualizacoes");

            migrationBuilder.CreateIndex(
                name: "IX_VisualizacoesIgreja_DataCriacao",
                table: "VisualizacoesIgreja",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_VisualizacoesIgreja_HashFingerprint",
                table: "VisualizacoesIgreja",
                column: "HashFingerprint");

            migrationBuilder.CreateIndex(
                name: "IX_VisualizacoesIgreja_IgrejaId",
                table: "VisualizacoesIgreja",
                column: "IgrejaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvaliacoesIgreja");

            migrationBuilder.DropTable(
                name: "ComentariosIgreja");

            migrationBuilder.DropTable(
                name: "CurtidasIgreja");

            migrationBuilder.DropTable(
                name: "EstatisticasEngajamentoIgreja");

            migrationBuilder.DropTable(
                name: "VisualizacoesIgreja");
        }
    }
}
