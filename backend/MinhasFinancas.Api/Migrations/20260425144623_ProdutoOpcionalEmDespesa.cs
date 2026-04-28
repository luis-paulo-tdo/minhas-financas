using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinhasFinancas.Api.Migrations
{
    /// <inheritdoc />
    public partial class ProdutoOpcionalEmDespesa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Despesas_Produtos_IdProduto",
                table: "Despesas");

            migrationBuilder.AlterColumn<int>(
                name: "IdProduto",
                table: "Despesas",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Despesas_Produtos_IdProduto",
                table: "Despesas",
                column: "IdProduto",
                principalTable: "Produtos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Despesas_Produtos_IdProduto",
                table: "Despesas");

            migrationBuilder.AlterColumn<int>(
                name: "IdProduto",
                table: "Despesas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Despesas_Produtos_IdProduto",
                table: "Despesas",
                column: "IdProduto",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
