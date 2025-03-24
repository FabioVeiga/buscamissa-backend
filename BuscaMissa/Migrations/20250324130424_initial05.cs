using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuscaMissa.Migrations
{
    /// <inheritdoc />
    public partial class initial05 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Solicitacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Assunto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mensagem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataSolicitacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NomeSolicitante = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailSolicitante = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Resolvido = table.Column<bool>(type: "bit", nullable: false),
                    Solucao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Resposta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnviarResposta = table.Column<bool>(type: "bit", nullable: true),
                    DataSolucao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitacoes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Solicitacoes");
        }
    }
}
