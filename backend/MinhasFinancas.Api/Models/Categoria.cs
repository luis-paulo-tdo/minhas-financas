namespace MinhasFinancas.Api.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    public ICollection<Despesa> Despesas { get; set; } = [];
}
