using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuscaMissa.Migrations
{
    /// <inheritdoc />
    public partial class fase1_slug_confianca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotente: adiciona colunas apenas se não existirem (MySQL não faz rollback DDL)
            migrationBuilder.Sql(@"
                SET @dbname = DATABASE();
                SET @tblname = 'Missas';
                SET @colname = 'FontePrincipal';
                SET @colexists = (SELECT COUNT(*) FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tblname AND COLUMN_NAME = @colname);
                SET @sql = IF(@colexists = 0,
                    'ALTER TABLE Missas ADD COLUMN FontePrincipal INT NOT NULL DEFAULT 0',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @dbname = DATABASE();
                SET @tblname = 'Missas';
                SET @colname = 'UltimaValidacao';
                SET @colexists = (SELECT COUNT(*) FROM information_schema.COLUMNS
                    WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tblname AND COLUMN_NAME = @colname);
                SET @sql = IF(@colexists = 0,
                    'ALTER TABLE Missas ADD COLUMN UltimaValidacao DATETIME(6) NULL',
                    'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "NomeUnico",
                table: "Igrejas",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<double>(
                name: "MediaAvaliacoes",
                table: "EstatisticasEngajamentoIgreja",
                type: "double",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Enderecos",
                type: "decimal(10,7)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Enderecos",
                type: "decimal(10,7)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            // Resolve duplicatas de NomeUnico adicionando sufixo numérico antes de criar o índice único
            migrationBuilder.Sql(@"
                SET @rank := 0;
                SET @prev := '';
                UPDATE Igrejas i
                JOIN (
                    SELECT Id,
                           NomeUnico,
                           @rank := IF(@prev = NomeUnico, @rank + 1, 1) AS rn,
                           @prev := NomeUnico
                    FROM Igrejas
                    WHERE NomeUnico IS NOT NULL
                    ORDER BY NomeUnico, Id
                ) ranked ON i.Id = ranked.Id
                SET i.NomeUnico = CASE
                    WHEN ranked.rn = 1 THEN ranked.NomeUnico
                    ELSE CONCAT(ranked.NomeUnico, '-', ranked.rn)
                END
                WHERE ranked.NomeUnico IS NOT NULL;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Igrejas_NomeUnico",
                table: "Igrejas",
                column: "NomeUnico",
                unique: true,
                filter: "NomeUnico IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Enderecos_Latitude_Longitude",
                table: "Enderecos",
                columns: new[] { "Latitude", "Longitude" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Igrejas_NomeUnico",
                table: "Igrejas");

            migrationBuilder.DropIndex(
                name: "IX_Enderecos_Latitude_Longitude",
                table: "Enderecos");

            migrationBuilder.DropColumn(
                name: "FontePrincipal",
                table: "Missas");

            migrationBuilder.DropColumn(
                name: "UltimaValidacao",
                table: "Missas");

            migrationBuilder.AlterColumn<string>(
                name: "NomeUnico",
                table: "Igrejas",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<decimal>(
                name: "MediaAvaliacoes",
                table: "EstatisticasEngajamentoIgreja",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<string>(
                name: "Longitude",
                table: "Enderecos",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,7)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Latitude",
                table: "Enderecos",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,7)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
