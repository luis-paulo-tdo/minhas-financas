namespace MinhasFinancas.Api.Models;

public class Produto
{
    public int    Id             { get; set; }
    public int?   IdMarca        { get; set; }
    public int?   IdLinhaProduto { get; set; }
    public string Nome           { get; set; } = string.Empty;

    public Marca?        Marca         { get; set; }
    public LinhaProduto? LinhaProduto  { get; set; }
    public ICollection<Despesa> Despesas { get; set; } = [];
}
