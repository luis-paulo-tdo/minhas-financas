using System.ComponentModel.DataAnnotations;

namespace MinhasFinancas.Api.DTOs;

public class LinhaProdutoRequest
{
    [Required, MinLength(2)]
    public string Nome { get; set; } = string.Empty;

    [Required, Range(1, int.MaxValue, ErrorMessage = "Marca obrigatória.")]
    public int IdMarca { get; set; }
}
