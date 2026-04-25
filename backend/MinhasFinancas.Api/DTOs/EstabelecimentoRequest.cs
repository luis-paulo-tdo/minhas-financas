using System.ComponentModel.DataAnnotations;

namespace MinhasFinancas.Api.DTOs;

public class EstabelecimentoRequest
{
    [Required, MinLength(2)]
    public string Nome { get; set; } = string.Empty;
}
