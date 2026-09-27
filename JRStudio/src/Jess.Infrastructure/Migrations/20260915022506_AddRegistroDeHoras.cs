using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jess.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistroDeHoras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistrosDeHoras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjetoId = table.Column<int>(type: "INTEGER", nullable: false),
                    InicioSessao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FimSessao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DuracaoMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    Origem = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Categoria = table.Column<int>(type: "INTEGER", nullable: true),
                    Nota = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosDeHoras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosDeHoras_Projetos_ProjetoId",
                        column: x => x.ProjetoId,
                        principalTable: "Projetos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PausasRegistroHoras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RegistroHorasId = table.Column<int>(type: "INTEGER", nullable: false),
                    InicioPausa = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FimPausa = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PausasRegistroHoras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PausasRegistroHoras_RegistrosDeHoras_RegistroHorasId",
                        column: x => x.RegistroHorasId,
                        principalTable: "RegistrosDeHoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentosHoras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RegistroHorasId = table.Column<int>(type: "INTEGER", nullable: false),
                    DuracaoMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    Categoria = table.Column<int>(type: "INTEGER", nullable: false),
                    Nota = table.Column<string>(type: "TEXT", nullable: true),
                    Ordem = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentosHoras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SegmentosHoras_RegistrosDeHoras_RegistroHorasId",
                        column: x => x.RegistroHorasId,
                        principalTable: "RegistrosDeHoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PausasRegistroHoras_RegistroHorasId",
                table: "PausasRegistroHoras",
                column: "RegistroHorasId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosDeHoras_ProjetoId",
                table: "RegistrosDeHoras",
                column: "ProjetoId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentosHoras_RegistroHorasId",
                table: "SegmentosHoras",
                column: "RegistroHorasId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PausasRegistroHoras");

            migrationBuilder.DropTable(
                name: "SegmentosHoras");

            migrationBuilder.DropTable(
                name: "RegistrosDeHoras");
        }
    }
}
