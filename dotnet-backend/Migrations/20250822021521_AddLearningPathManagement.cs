using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TutorCopiloto.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningPathManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningPaths",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", nullable: false),
                    Dificuldade = table.Column<string>(type: "TEXT", nullable: false),
                    OrdemSequencia = table.Column<int>(type: "INTEGER", nullable: false),
                    DuracaoEstimadaHoras = table.Column<int>(type: "INTEGER", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningPaths", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearningModules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    LearningPathId = table.Column<string>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ConteudoMarkdown = table.Column<string>(type: "TEXT", nullable: false),
                    Ordem = table.Column<int>(type: "INTEGER", nullable: false),
                    ObrigatorioParaProgresso = table.Column<bool>(type: "INTEGER", nullable: false),
                    TempoEstimadoMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    RecursosAdicionais = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningModules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningModules_LearningPaths_LearningPathId",
                        column: x => x.LearningPathId,
                        principalTable: "LearningPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentProgresses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LearningPathId = table.Column<string>(type: "TEXT", nullable: false),
                    IniciadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConcluidoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ProgressoPercentual = table.Column<double>(type: "REAL", precision: 5, scale: 2, nullable: false),
                    TempoGastoMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    UltimaAtividadeEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProgresses_LearningPaths_LearningPathId",
                        column: x => x.LearningPathId,
                        principalTable: "LearningPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentProgresses_Usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModuleCompletions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    StudentProgressId = table.Column<string>(type: "TEXT", nullable: false),
                    LearningModuleId = table.Column<string>(type: "TEXT", nullable: false),
                    ConcluidoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TempoGastoMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    NotaAvaliacao = table.Column<double>(type: "REAL", precision: 5, scale: 2, nullable: true),
                    FeedbackEstudante = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleCompletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleCompletions_LearningModules_LearningModuleId",
                        column: x => x.LearningModuleId,
                        principalTable: "LearningModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModuleCompletions_StudentProgresses_StudentProgressId",
                        column: x => x.StudentProgressId,
                        principalTable: "StudentProgresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AvaliacoesCodigo",
                keyColumn: "Id",
                keyValue: "demo-assessment-1",
                column: "CriadoEm",
                value: new DateTime(2025, 8, 22, 2, 15, 21, 442, DateTimeKind.Utc).AddTicks(9898));

            migrationBuilder.UpdateData(
                table: "Interacoes",
                keyColumn: "Id",
                keyValue: "demo-interaction-1",
                column: "CriadoEm",
                value: new DateTime(2025, 8, 22, 2, 15, 21, 442, DateTimeKind.Utc).AddTicks(9862));

            migrationBuilder.InsertData(
                table: "LearningPaths",
                columns: new[] { "Id", "Ativo", "AtualizadoEm", "Categoria", "CriadoEm", "Descricao", "Dificuldade", "DuracaoEstimadaHoras", "Nome", "OrdemSequencia" },
                values: new object[,]
                {
                    { "lp-javascript-basics", true, null, "Frontend", new DateTime(2025, 8, 22, 2, 15, 21, 442, DateTimeKind.Utc).AddTicks(9927), "Aprenda os conceitos fundamentais de JavaScript do zero", "Beginner", 20, "JavaScript Fundamentals", 1 },
                    { "lp-react-fundamentals", true, null, "Frontend", new DateTime(2025, 8, 22, 2, 15, 21, 442, DateTimeKind.Utc).AddTicks(9930), "Domine os conceitos básicos do React para desenvolvimento frontend", "Intermediate", 35, "React Fundamentals", 2 }
                });

            migrationBuilder.UpdateData(
                table: "Sessoes",
                keyColumn: "Id",
                keyValue: "demo-session-1",
                columns: new[] { "CriadoEm", "FinalizadoEm" },
                values: new object[] { new DateTime(2025, 8, 22, 2, 15, 21, 442, DateTimeKind.Utc).AddTicks(9817), new DateTime(2025, 8, 22, 2, 45, 21, 442, DateTimeKind.Utc).AddTicks(9818) });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: "demo-user-1",
                columns: new[] { "CriadoEm", "UltimoAcesso" },
                values: new object[] { new DateTime(2025, 8, 22, 2, 15, 21, 442, DateTimeKind.Utc).AddTicks(9662), new DateTime(2025, 8, 22, 2, 15, 21, 442, DateTimeKind.Utc).AddTicks(9664) });

            migrationBuilder.InsertData(
                table: "LearningModules",
                columns: new[] { "Id", "ConteudoMarkdown", "CriadoEm", "Descricao", "LearningPathId", "Nome", "ObrigatorioParaProgresso", "Ordem", "RecursosAdicionais", "TempoEstimadoMinutos" },
                values: new object[,]
                {
                    { "mod-js-functions", "# Funções em JavaScript\n\nFunções são blocos de código reutilizáveis...", new DateTime(2025, 8, 22, 2, 15, 21, 442, DateTimeKind.Utc).AddTicks(9966), "Aprenda a criar e usar funções em JavaScript", "lp-javascript-basics", "Funções", true, 2, "https://developer.mozilla.org/pt-BR/docs/Web/JavaScript/Guide/Functions", 60 },
                    { "mod-js-variables", "# Variáveis em JavaScript\n\nVariáveis são containers para armazenar dados...", new DateTime(2025, 8, 22, 2, 15, 21, 442, DateTimeKind.Utc).AddTicks(9964), "Entenda como declarar e usar variáveis em JavaScript", "lp-javascript-basics", "Variáveis e Tipos de Dados", true, 1, "https://developer.mozilla.org/pt-BR/docs/Web/JavaScript/Guide/Grammar_and_types", 45 },
                    { "mod-react-intro", "# Introdução ao React\n\nReact é uma biblioteca JavaScript para construir interfaces de usuário...", new DateTime(2025, 8, 22, 2, 15, 21, 442, DateTimeKind.Utc).AddTicks(9969), "Conceitos básicos e configuração do ambiente React", "lp-react-fundamentals", "Introdução ao React", true, 1, "https://react.dev/learn", 90 }
                });

            migrationBuilder.InsertData(
                table: "StudentProgresses",
                columns: new[] { "Id", "ConcluidoEm", "IniciadoEm", "LearningPathId", "ProgressoPercentual", "Status", "TempoGastoMinutos", "UltimaAtividadeEm", "UserId" },
                values: new object[] { "progress-demo-1", null, new DateTime(2025, 8, 17, 2, 15, 21, 443, DateTimeKind.Utc), "lp-javascript-basics", 50.0, "InProgress", 45, new DateTime(2025, 8, 22, 0, 15, 21, 443, DateTimeKind.Utc).AddTicks(3), "demo-user-1" });

            migrationBuilder.InsertData(
                table: "ModuleCompletions",
                columns: new[] { "Id", "ConcluidoEm", "FeedbackEstudante", "LearningModuleId", "NotaAvaliacao", "StudentProgressId", "TempoGastoMinutos" },
                values: new object[] { "completion-demo-1", new DateTime(2025, 8, 19, 2, 15, 21, 443, DateTimeKind.Utc).AddTicks(37), "Conteúdo muito claro e bem explicado!", "mod-js-variables", 8.5, "progress-demo-1", 45 });

            migrationBuilder.CreateIndex(
                name: "IX_LearningModules_LearningPathId",
                table: "LearningModules",
                column: "LearningPathId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningModules_LearningPathId_Ordem",
                table: "LearningModules",
                columns: new[] { "LearningPathId", "Ordem" });

            migrationBuilder.CreateIndex(
                name: "IX_LearningModules_Ordem",
                table: "LearningModules",
                column: "Ordem");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPaths_Ativo",
                table: "LearningPaths",
                column: "Ativo");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPaths_Categoria",
                table: "LearningPaths",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPaths_Categoria_Dificuldade_Ativo",
                table: "LearningPaths",
                columns: new[] { "Categoria", "Dificuldade", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_LearningPaths_Dificuldade",
                table: "LearningPaths",
                column: "Dificuldade");

            migrationBuilder.CreateIndex(
                name: "IX_LearningPaths_OrdemSequencia",
                table: "LearningPaths",
                column: "OrdemSequencia");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleCompletion_StudentProgressId_ConcluidoEm",
                table: "ModuleCompletions",
                columns: new[] { "StudentProgressId", "ConcluidoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleCompletions_ConcluidoEm",
                table: "ModuleCompletions",
                column: "ConcluidoEm");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleCompletions_LearningModuleId",
                table: "ModuleCompletions",
                column: "LearningModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleCompletions_StudentProgressId",
                table: "ModuleCompletions",
                column: "StudentProgressId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleCompletions_StudentProgressId_LearningModuleId",
                table: "ModuleCompletions",
                columns: new[] { "StudentProgressId", "LearningModuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgress_UserId_Status_UltimaAtividade",
                table: "StudentProgresses",
                columns: new[] { "UserId", "Status", "UltimaAtividadeEm" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgresses_LearningPathId",
                table: "StudentProgresses",
                column: "LearningPathId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgresses_Status",
                table: "StudentProgresses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgresses_UserId",
                table: "StudentProgresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgresses_UserId_LearningPathId",
                table: "StudentProgresses",
                columns: new[] { "UserId", "LearningPathId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleCompletions");

            migrationBuilder.DropTable(
                name: "LearningModules");

            migrationBuilder.DropTable(
                name: "StudentProgresses");

            migrationBuilder.DropTable(
                name: "LearningPaths");

            migrationBuilder.UpdateData(
                table: "AvaliacoesCodigo",
                keyColumn: "Id",
                keyValue: "demo-assessment-1",
                column: "CriadoEm",
                value: new DateTime(2025, 8, 20, 4, 36, 24, 68, DateTimeKind.Utc).AddTicks(9916));

            migrationBuilder.UpdateData(
                table: "Interacoes",
                keyColumn: "Id",
                keyValue: "demo-interaction-1",
                column: "CriadoEm",
                value: new DateTime(2025, 8, 20, 4, 36, 24, 68, DateTimeKind.Utc).AddTicks(9878));

            migrationBuilder.UpdateData(
                table: "Sessoes",
                keyColumn: "Id",
                keyValue: "demo-session-1",
                columns: new[] { "CriadoEm", "FinalizadoEm" },
                values: new object[] { new DateTime(2025, 8, 20, 4, 36, 24, 68, DateTimeKind.Utc).AddTicks(9813), new DateTime(2025, 8, 20, 5, 6, 24, 68, DateTimeKind.Utc).AddTicks(9813) });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: "demo-user-1",
                columns: new[] { "CriadoEm", "UltimoAcesso" },
                values: new object[] { new DateTime(2025, 8, 20, 4, 36, 24, 68, DateTimeKind.Utc).AddTicks(9473), new DateTime(2025, 8, 20, 4, 36, 24, 68, DateTimeKind.Utc).AddTicks(9474) });
        }
    }
}
