using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuscaMissa.Migrations
{
    /// <inheritdoc />
    public partial class criar_email_eventos_igreja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailEventosIgreja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IgrejaId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Assunto = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailDestino = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NomeDestino = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Html = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Enviado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Observacao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailEventosIgreja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailEventosIgreja_Igrejas_IgrejaId",
                        column: x => x.IgrejaId,
                        principalTable: "Igrejas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EmailEventosIgreja_DataCriacao",
                table: "EmailEventosIgreja",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_EmailEventosIgreja_EmailDestino",
                table: "EmailEventosIgreja",
                column: "EmailDestino");

            migrationBuilder.CreateIndex(
                name: "IX_EmailEventosIgreja_IgrejaId",
                table: "EmailEventosIgreja",
                column: "IgrejaId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailEventosIgreja_IgrejaId_Tipo",
                table: "EmailEventosIgreja",
                columns: new[] { "IgrejaId", "Tipo" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailEventosIgreja_Tipo",
                table: "EmailEventosIgreja",
                column: "Tipo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailEventosIgreja");
        }
    }
}
