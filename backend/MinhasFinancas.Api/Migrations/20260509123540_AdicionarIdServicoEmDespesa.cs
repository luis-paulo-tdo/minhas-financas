using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinhasFinancas.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarIdServicoEmDespesa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdServico",
                table: "Despesas",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Despesas_IdServico",
                table: "Despesas",
                column: "IdServico");

            migrationBuilder.AddForeignKey(
                name: "FK_Despesas_Servicos_IdServico",
                table: "Despesas",
                column: "IdServico",
                principalTable: "Servicos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Despesas_Servicos_IdServico",
                table: "Despesas");

            migrationBuilder.DropIndex(
                name: "IX_Despesas_IdServico",
                table: "Despesas");

            migrationBuilder.DropColumn(
                name: "IdServico",
                table: "Despesas");
        }
    }
}
