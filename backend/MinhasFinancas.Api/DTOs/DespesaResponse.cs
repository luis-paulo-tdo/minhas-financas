namespace MinhasFinancas.Api.DTOs;

public class DespesaResponse
{
    public int      Id                  { get; set; }
    public int      IdCategoria         { get; set; }
    public string   NomeCategoria       { get; set; } = string.Empty;
    public int      IdEstabelecimento   { get; set; }
    public string   NomeEstabelecimento { get; set; } = string.Empty;
    public int      IdProduto           { get; set; }
    public string   NomeProduto         { get; set; } = string.Empty;
    public int      IdMarca             { get; set; }
    public string   NomeMarca           { get; set; } = string.Empty;
    public string?  Descricao           { get; set; }
    public decimal  Valor               { get; set; }
    public DateTime DataCriacao         { get; set; }
}
