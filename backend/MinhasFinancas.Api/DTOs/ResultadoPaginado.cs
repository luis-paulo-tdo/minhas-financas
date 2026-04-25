namespace MinhasFinancas.Api.DTOs;

public class ResultadoPaginado<T>
{
    public IEnumerable<T> Itens        { get; set; } = [];
    public int            Total        { get; set; }
    public int            Pagina       { get; set; }
    public int            TotalPaginas { get; set; }
}
