namespace MinhasFinancas.Api.Models;

public class LinhaProduto
{
    public int    Id      { get; set; }
    public int    IdMarca { get; set; }
    public string Nome    { get; set; } = string.Empty;

    public Marca             Marca    { get; set; } = null!;
    public ICollection<Produto> Produtos { get; set; } = [];
}
