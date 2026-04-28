using System.ComponentModel.DataAnnotations;

namespace MinhasFinancas.Api.DTOs;

public class DespesaRequest
{
    [Required]
    public int IdCategoria { get; set; }

    [Required]
    public int IdEstabelecimento { get; set; }

    public int? IdProduto { get; set; }

    public string? Descricao { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
    public decimal Valor { get; set; }

    [Range(0.001, double.MaxValue, ErrorMessage = "O preço a granel deve ser maior que zero.")]
    public decimal? PrecoGranel { get; set; }

    public string? UnidadeGranel { get; set; }

    public DateTime? DataCriacao { get; set; }
}
