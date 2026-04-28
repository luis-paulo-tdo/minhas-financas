using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinhasFinancas.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarLinhaProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdLinhaProduto",
                table: "Produtos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LinhasProduto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdMarca = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinhasProduto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinhasProduto_Marcas_IdMarca",
                        column: x => x.IdMarca,
                        principalTable: "Marcas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_IdLinhaProduto",
                table: "Produtos",
                column: "IdLinhaProduto");

            migrationBuilder.CreateIndex(
                name: "IX_LinhasProduto_IdMarca_Nome",
                table: "LinhasProduto",
                columns: new[] { "IdMarca", "Nome" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Produtos_LinhasProduto_IdLinhaProduto",
                table: "Produtos",
                column: "IdLinhaProduto",
                principalTable: "LinhasProduto",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produtos_LinhasProduto_IdLinhaProduto",
                table: "Produtos");

            migrationBuilder.DropTable(
                name: "LinhasProduto");

            migrationBuilder.DropIndex(
                name: "IX_Produtos_IdLinhaProduto",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "IdLinhaProduto",
                table: "Produtos");
        }
    }
}
