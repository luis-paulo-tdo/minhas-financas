namespace MinhasFinancas.Api.DTOs;

public class ProdutoResponse
{
    public int     Id               { get; set; }
    public string  Nome             { get; set; } = string.Empty;
    public int?    IdMarca          { get; set; }
    public string? NomeMarca        { get; set; }
    public int?    IdLinhaProduto   { get; set; }
    public string? NomeLinhaProduto { get; set; }
}
