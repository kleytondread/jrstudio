using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jess.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddArquivoETag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Arquivos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NomeArquivo = table.Column<string>(type: "TEXT", nullable: false),
                    CaminhoArquivo = table.Column<string>(type: "TEXT", nullable: false),
                    TipoArquivo = table.Column<int>(type: "INTEGER", nullable: false),
                    TamanhoBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    DataImportacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Favorito = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arquivos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArquivoProjeto",
                columns: table => new
                {
                    ArquivoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjetosId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArquivoProjeto", x => new { x.ArquivoId, x.ProjetosId });
                    table.ForeignKey(
                        name: "FK_ArquivoProjeto_Arquivos_ArquivoId",
                        column: x => x.ArquivoId,
                        principalTable: "Arquivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArquivoProjeto_Projetos_ProjetosId",
                        column: x => x.ProjetosId,
                        principalTable: "Projetos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArquivoTag",
                columns: table => new
                {
                    ArquivosId = table.Column<int>(type: "INTEGER", nullable: false),
                    TagsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArquivoTag", x => new { x.ArquivosId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_ArquivoTag_Arquivos_ArquivosId",
                        column: x => x.ArquivosId,
                        principalTable: "Arquivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArquivoTag_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArquivoProjeto_ProjetosId",
                table: "ArquivoProjeto",
                column: "ProjetosId");

            migrationBuilder.CreateIndex(
                name: "IX_ArquivoTag_TagsId",
                table: "ArquivoTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Nome",
                table: "Tags",
                column: "Nome",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArquivoProjeto");

            migrationBuilder.DropTable(
                name: "ArquivoTag");

            migrationBuilder.DropTable(
                name: "Arquivos");

            migrationBuilder.DropTable(
                name: "Tags");
        }
    }
}
