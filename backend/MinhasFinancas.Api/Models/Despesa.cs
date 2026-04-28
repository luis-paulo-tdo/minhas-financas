namespace MinhasFinancas.Api.Models;

public class Despesa
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public int IdCategoria { get; set; }
    public int IdEstabelecimento { get; set; }
    public int? IdProduto { get; set; }
    public string? Descricao { get; set; }
    public decimal Valor { get; set; }
    public decimal? PrecoGranel { get; set; }
    public string? UnidadeGranel { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public Usuario Usuario { get; set; } = null!;
    public Categoria Categoria { get; set; } = null!;
    public Estabelecimento Estabelecimento { get; set; } = null!;
    public Produto? Produto { get; set; }
}
