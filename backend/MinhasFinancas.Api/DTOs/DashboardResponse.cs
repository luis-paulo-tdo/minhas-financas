namespace MinhasFinancas.Api.DTOs;

public class DashboardResponse
{
    public ResumoDespesas              Resumo              { get; set; } = new();
    public IEnumerable<EvolucaoMensalItem> EvolucaoMensal  { get; set; } = [];
    public IEnumerable<RankingItem>    TopEstabelecimentos { get; set; } = [];
    public IEnumerable<RankingItem>    TopProdutos         { get; set; } = [];
}

public class EvolucaoMensalItem
{
    public int     Ano               { get; set; }
    public int     Mes               { get; set; }
    public decimal TotalEssencial    { get; set; }
    public decimal TotalLazer        { get; set; }
    public decimal TotalInvestimento { get; set; }
}

public class RankingItem
{
    public string  Nome    { get; set; } = string.Empty;
    public string? Detalhe { get; set; }
    public decimal Total   { get; set; }
}
