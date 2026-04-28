using System.ComponentModel.DataAnnotations;

namespace MinhasFinancas.Api.DTOs;

public class ProdutoRequest
{
    [Required, MinLength(2)]
    public string Nome { get; set; } = string.Empty;

    public int? IdMarca { get; set; }

    public int? IdLinhaProduto { get; set; }
}
