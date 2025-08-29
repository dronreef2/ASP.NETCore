using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutorCopiloto.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    TurmaId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UltimoAcesso = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AvaliacoesCodigo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Tema = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NotaFinal = table.Column<double>(type: "REAL", precision: 5, scale: 2, nullable: false),
                    Feedback = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvaliacoesCodigo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvaliacoesCodigo_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessoes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FinalizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DuracaoMinutos = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessoes_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Interacoes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    SessaoId = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FerramentaUsada = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Sucesso = table.Column<bool>(type: "INTEGER", nullable: false),
                    MensagemErro = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    TempoExecucaoMs = table.Column<int>(type: "INTEGER", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interacoes_Sessoes_SessaoId",
                        column: x => x.SessaoId,
                        principalTable: "Sessoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "CriadoEm", "Email", "Nome", "TurmaId", "UltimoAcesso" },
                values: new object[] { "demo-user-1", new DateTime(2025, 8, 20, 4, 36, 24, 68, DateTimeKind.Utc).AddTicks(9473), "demo@tutorcopiloto.com", "Usuário Demo", "turma-demo", new DateTime(2025, 8, 20, 4, 36, 24, 68, DateTimeKind.Utc).AddTicks(9474) });

            migrationBuilder.InsertData(
                table: "AvaliacoesCodigo",
                columns: new[] { "Id", "CriadoEm", "Feedback", "NotaFinal", "Tema", "UserId" },
                values: new object[] { "demo-assessment-1", new DateTime(2025, 8, 20, 4, 36, 24, 68, DateTimeKind.Utc).AddTicks(9916), "Bom entendimento dos conceitos fundamentais. Continue praticando loops e condicionais.", 8.5, "JavaScript Básico", "demo-user-1" });

            migrationBuilder.InsertData(
                table: "Sessoes",
                columns: new[] { "Id", "CriadoEm", "DuracaoMinutos", "FinalizadoEm", "UserId" },
                values: new object[] { "demo-session-1", new DateTime(2025, 8, 20, 4, 36, 24, 68, DateTimeKind.Utc).AddTicks(9813), 30, new DateTime(2025, 8, 20, 5, 6, 24, 68, DateTimeKind.Utc).AddTicks(9813), "demo-user-1" });

            migrationBuilder.InsertData(
                table: "Interacoes",
                columns: new[] { "Id", "CriadoEm", "FerramentaUsada", "MensagemErro", "SessaoId", "Sucesso", "TempoExecucaoMs", "Tipo" },
                values: new object[] { "demo-interaction-1", new DateTime(2025, 8, 20, 4, 36, 24, 68, DateTimeKind.Utc).AddTicks(9878), "code-analyzer", null, "demo-session-1", true, 1500, "explain" });

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesCodigo_CriadoEm",
                table: "AvaliacoesCodigo",
                column: "CriadoEm");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesCodigo_Tema",
                table: "AvaliacoesCodigo",
                column: "Tema");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesCodigo_UserId",
                table: "AvaliacoesCodigo",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesCodigo_UserId_Tema_CriadoEm",
                table: "AvaliacoesCodigo",
                columns: new[] { "UserId", "Tema", "CriadoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_Interacoes_CriadoEm",
                table: "Interacoes",
                column: "CriadoEm");

            migrationBuilder.CreateIndex(
                name: "IX_Interacoes_FerramentaUsada",
                table: "Interacoes",
                column: "FerramentaUsada");

            migrationBuilder.CreateIndex(
                name: "IX_Interacoes_SessaoId",
                table: "Interacoes",
                column: "SessaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Interacoes_SessaoId_Tipo_CriadoEm",
                table: "Interacoes",
                columns: new[] { "SessaoId", "Tipo", "CriadoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_Interacoes_Tipo",
                table: "Interacoes",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_Sessoes_CriadoEm",
                table: "Sessoes",
                column: "CriadoEm");

            migrationBuilder.CreateIndex(
                name: "IX_Sessoes_UserId",
                table: "Sessoes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessoes_UserId_CriadoEm",
                table: "Sessoes",
                columns: new[] { "UserId", "CriadoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TurmaId",
                table: "Usuarios",
                column: "TurmaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvaliacoesCodigo");

            migrationBuilder.DropTable(
                name: "Interacoes");

            migrationBuilder.DropTable(
                name: "Sessoes");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
