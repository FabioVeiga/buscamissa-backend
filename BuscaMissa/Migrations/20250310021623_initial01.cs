using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuscaMissa.Migrations
{
    /// <inheritdoc />
    public partial class initial01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IgrejaTemporarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Paroco = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImagemUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IgrejaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IgrejaTemporarias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Perfil = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Senha = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AceitarTermo = table.Column<bool>(type: "bit", nullable: false),
                    AceitarPromocao = table.Column<bool>(type: "bit", nullable: true),
                    Criacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Igrejas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Paroco = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImagemUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Criacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Alteracao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Igrejas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Igrejas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Contatos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailContato = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailContatoValidado = table.Column<bool>(type: "bit", nullable: true),
                    DDD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TelefoneValidado = table.Column<bool>(type: "bit", nullable: true),
                    DDDWhatsApp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TelefoneWhatsApp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TelefoneWhatsAppValidado = table.Column<bool>(type: "bit", nullable: true),
                    IgrejaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contatos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contatos_Igrejas_IgrejaId",
                        column: x => x.IgrejaId,
                        principalTable: "Igrejas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Controles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IgrejaId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Controles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Controles_Igrejas_IgrejaId",
                        column: x => x.IgrejaId,
                        principalTable: "Igrejas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Enderecos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cep = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Logradouro = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Complemento = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bairro = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Localidade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Uf = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Regiao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IgrejaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enderecos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enderecos_Igrejas_IgrejaId",
                        column: x => x.IgrejaId,
                        principalTable: "Igrejas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Missas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    Horario = table.Column<TimeSpan>(type: "time", nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IgrejaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Missas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Missas_Igrejas_IgrejaId",
                        column: x => x.IgrejaId,
                        principalTable: "Igrejas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MissasTemporarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    Horario = table.Column<TimeSpan>(type: "time", nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IgrejaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissasTemporarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissasTemporarias_Igrejas_IgrejaId",
                        column: x => x.IgrejaId,
                        principalTable: "Igrejas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RedesSociais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoRedeSocial = table.Column<int>(type: "int", nullable: false),
                    NomeDoPerfil = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Verificado = table.Column<bool>(type: "bit", nullable: false),
                    IgrejaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedesSociais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RedesSociais_Igrejas_IgrejaId",
                        column: x => x.IgrejaId,
                        principalTable: "Igrejas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CodigoPermissoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoToken = table.Column<int>(type: "int", nullable: false),
                    ValidoAte = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ControleId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodigoPermissoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodigoPermissoes_Controles_ControleId",
                        column: x => x.ControleId,
                        principalTable: "Controles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodigoPermissoes_ControleId",
                table: "CodigoPermissoes",
                column: "ControleId");

            migrationBuilder.CreateIndex(
                name: "IX_Contatos_IgrejaId",
                table: "Contatos",
                column: "IgrejaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Controles_IgrejaId",
                table: "Controles",
                column: "IgrejaId");

            migrationBuilder.CreateIndex(
                name: "IX_Enderecos_IgrejaId",
                table: "Enderecos",
                column: "IgrejaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Igrejas_UsuarioId",
                table: "Igrejas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Missas_IgrejaId",
                table: "Missas",
                column: "IgrejaId");

            migrationBuilder.CreateIndex(
                name: "IX_MissasTemporarias_IgrejaId",
                table: "MissasTemporarias",
                column: "IgrejaId");

            migrationBuilder.CreateIndex(
                name: "IX_RedesSociais_IgrejaId",
                table: "RedesSociais",
                column: "IgrejaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodigoPermissoes");

            migrationBuilder.DropTable(
                name: "Contatos");

            migrationBuilder.DropTable(
                name: "Enderecos");

            migrationBuilder.DropTable(
                name: "IgrejaTemporarias");

            migrationBuilder.DropTable(
                name: "Missas");

            migrationBuilder.DropTable(
                name: "MissasTemporarias");

            migrationBuilder.DropTable(
                name: "RedesSociais");

            migrationBuilder.DropTable(
                name: "Controles");

            migrationBuilder.DropTable(
                name: "Igrejas");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
