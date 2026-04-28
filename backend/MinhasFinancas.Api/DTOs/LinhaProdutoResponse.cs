namespace MinhasFinancas.Api.DTOs;

public class LinhaProdutoResponse
{
    public int    Id        { get; set; }
    public string Nome      { get; set; } = string.Empty;
    public int    IdMarca   { get; set; }
    public string NomeMarca { get; set; } = string.Empty;
}
