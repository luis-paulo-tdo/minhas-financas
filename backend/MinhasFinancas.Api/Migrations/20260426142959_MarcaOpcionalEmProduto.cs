using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinhasFinancas.Api.Migrations
{
    /// <inheritdoc />
    public partial class MarcaOpcionalEmProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produtos_Marcas_IdMarca",
                table: "Produtos");

            migrationBuilder.AlterColumn<int>(
                name: "IdMarca",
                table: "Produtos",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Produtos_Marcas_IdMarca",
                table: "Produtos",
                column: "IdMarca",
                principalTable: "Marcas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produtos_Marcas_IdMarca",
                table: "Produtos");

            migrationBuilder.AlterColumn<int>(
                name: "IdMarca",
                table: "Produtos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Produtos_Marcas_IdMarca",
                table: "Produtos",
                column: "IdMarca",
                principalTable: "Marcas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
