using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinhasFinancas.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPrecoGranel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecoGranel",
                table: "Despesas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnidadeGranel",
                table: "Despesas",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecoGranel",
                table: "Despesas");

            migrationBuilder.DropColumn(
                name: "UnidadeGranel",
                table: "Despesas");
        }
    }
}
