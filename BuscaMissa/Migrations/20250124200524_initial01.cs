using System;
using Microsoft.EntityFrameworkCore.Metadata;
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
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "IgrejaTemporarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Paroco = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImagemUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IgrejaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IgrejaTemporarias", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Perfil = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Senha = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AceitarTermo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AceitarPromocao = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    Criacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Igrejas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Paroco = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImagemUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Criacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Alteracao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Contatos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmailContato = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailContatoValidado = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    DDD = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TelefoneValidado = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    DDDWhatsApp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TelefoneWhatsApp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TelefoneWhatsAppValidado = table.Column<bool>(type: "tinyint(1)", nullable: true),
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Controles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Enderecos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Cep = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Logradouro = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Complemento = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Bairro = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Localidade = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Uf = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Regiao = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Missas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    Horario = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Observacao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MissasTemporarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    Horario = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Observacao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RedesSociais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TipoRedeSocial = table.Column<int>(type: "int", nullable: true),
                    Url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Site = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CodigoPermissoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CodigoToken = table.Column<int>(type: "int", nullable: false),
                    ValidoAte = table.Column<DateTime>(type: "datetime(6)", nullable: false),
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CodigoPermissoes_ControleId",
                table: "CodigoPermissoes",
                column: "ControleId");

            migrationBuilder.CreateIndex(
                name: "IX_Contatos_IgrejaId",
                table: "Contatos",
                column: "IgrejaId");

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
