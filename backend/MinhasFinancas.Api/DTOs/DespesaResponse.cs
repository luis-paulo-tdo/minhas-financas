namespace MinhasFinancas.Api.DTOs;

public class DespesaResponse
{
    public int      Id                  { get; set; }
    public int      IdCategoria         { get; set; }
    public string   NomeCategoria       { get; set; } = string.Empty;
    public int      IdEstabelecimento   { get; set; }
    public string   NomeEstabelecimento { get; set; } = string.Empty;
    public int?     IdProduto           { get; set; }
    public string?  NomeProduto         { get; set; }
    public int?     IdMarca             { get; set; }
    public string?  NomeMarca           { get; set; }
    public int?     IdLinhaProduto      { get; set; }
    public string?  NomeLinhaProduto    { get; set; }
    public string?  Descricao           { get; set; }
    public decimal  Valor               { get; set; }
    public decimal? PrecoGranel         { get; set; }
    public string?  UnidadeGranel       { get; set; }
    public DateTime DataCriacao         { get; set; }
}
