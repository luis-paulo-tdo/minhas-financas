namespace MinhasFinancas.Api.Models;

public class Marca
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    public ICollection<Produto>      Produtos      { get; set; } = [];
    public ICollection<LinhaProduto> LinhasProduto { get; set; } = [];
}
