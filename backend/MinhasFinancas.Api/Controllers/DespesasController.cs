using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Api.Data;
using MinhasFinancas.Api.DTOs;
using MinhasFinancas.Api.Models;

namespace MinhasFinancas.Api.Controllers;

[ApiController]
[Route("api/despesas")]
[Authorize]
public class DespesasController(AppDbContext db) : ControllerBase
{
    private int IdUsuario =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<ActionResult<ResultadoPaginado<DespesaResponse>>> Listar(
        [FromQuery] int?      idCategoria,
        [FromQuery] int?      idEstabelecimento,
        [FromQuery] int?      idProduto,
        [FromQuery] DateTime? de,
        [FromQuery] DateTime? ate,
        [FromQuery] int       pagina       = 1,
        [FromQuery] int       tamanhoPagina = 20)
    {
        var query = db.Despesas
            .Where(d => d.IdUsuario == IdUsuario)
            .AsQueryable();

        if (idCategoria.HasValue)
            query = query.Where(d => d.IdCategoria == idCategoria.Value);

        if (idEstabelecimento.HasValue)
            query = query.Where(d => d.IdEstabelecimento == idEstabelecimento.Value);

        if (idProduto.HasValue)
            query = query.Where(d => d.IdProduto == idProduto.Value);

        if (de.HasValue)
            query = query.Where(d => d.DataCriacao >= de.Value);

        if (ate.HasValue)
            query = query.Where(d => d.DataCriacao <= ate.Value.AddDays(1).AddTicks(-1));

        var total = await query.CountAsync();

        var itens = await query
            .OrderByDescending(d => d.DataCriacao)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(d => new DespesaResponse
            {
                Id                  = d.Id,
                IdCategoria         = d.IdCategoria,
                NomeCategoria       = d.Categoria.Nome,
                IdEstabelecimento   = d.IdEstabelecimento,
                NomeEstabelecimento = d.Estabelecimento.Nome,
                IdProduto           = d.IdProduto,
                NomeProduto         = d.Produto.Nome,
                IdMarca             = d.Produto.IdMarca,
                NomeMarca           = d.Produto.Marca.Nome,
                Descricao           = d.Descricao,
                Valor               = d.Valor,
                DataCriacao         = d.DataCriacao
            })
            .ToListAsync();

        return Ok(new ResultadoPaginado<DespesaResponse>
        {
            Itens        = itens,
            Total        = total,
            Pagina       = pagina,
            TotalPaginas = (int)Math.Ceiling((double)total / tamanhoPagina)
        });
    }

    [HttpGet("resumo")]
    public async Task<ActionResult<ResumoDespesas>> Resumo(
        [FromQuery] DateTime? de,
        [FromQuery] DateTime? ate)
    {
        var query = db.Despesas
            .Where(d => d.IdUsuario == IdUsuario)
            .AsQueryable();

        if (de.HasValue)
            query = query.Where(d => d.DataCriacao >= de.Value);

        if (ate.HasValue)
            query = query.Where(d => d.DataCriacao <= ate.Value.AddDays(1).AddTicks(-1));

        var totais = await query
            .GroupBy(d => d.Categoria.Nome)
            .Select(g => new { Categoria = g.Key, Total = g.Sum(d => d.Valor) })
            .ToListAsync();

        var resumo = new ResumoDespesas
        {
            TotalEssencial    = totais.FirstOrDefault(t => t.Categoria == "Essencial")?.Total    ?? 0,
            TotalLazer        = totais.FirstOrDefault(t => t.Categoria == "Lazer")?.Total        ?? 0,
            TotalInvestimento = totais.FirstOrDefault(t => t.Categoria == "Investimento")?.Total ?? 0,
        };
        resumo.TotalGeral = resumo.TotalEssencial + resumo.TotalLazer + resumo.TotalInvestimento;

        return Ok(resumo);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DespesaResponse>> Obter(int id)
    {
        var despesa = await db.Despesas
            .Where(d => d.Id == id && d.IdUsuario == IdUsuario)
            .Select(d => new DespesaResponse
            {
                Id                  = d.Id,
                IdCategoria         = d.IdCategoria,
                NomeCategoria       = d.Categoria.Nome,
                IdEstabelecimento   = d.IdEstabelecimento,
                NomeEstabelecimento = d.Estabelecimento.Nome,
                IdProduto           = d.IdProduto,
                NomeProduto         = d.Produto.Nome,
                IdMarca             = d.Produto.IdMarca,
                NomeMarca           = d.Produto.Marca.Nome,
                Descricao           = d.Descricao,
                Valor               = d.Valor,
                DataCriacao         = d.DataCriacao
            })
            .FirstOrDefaultAsync();

        if (despesa is null) return NotFound();

        return Ok(despesa);
    }

    [HttpPost]
    public async Task<ActionResult<DespesaResponse>> Criar(DespesaRequest req)
    {
        var erro = await ValidarRefs(req);
        if (erro is not null) return erro;

        var despesa = new Despesa
        {
            IdUsuario         = IdUsuario,
            IdCategoria       = req.IdCategoria,
            IdEstabelecimento = req.IdEstabelecimento,
            IdProduto         = req.IdProduto,
            Descricao         = req.Descricao,
            Valor             = req.Valor,
            DataCriacao       = req.DataCriacao ?? DateTime.UtcNow
        };

        db.Despesas.Add(despesa);
        await db.SaveChangesAsync();

        var response = await CarregarResponse(despesa.Id);
        return CreatedAtAction(nameof(Obter), new { id = despesa.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DespesaResponse>> Atualizar(int id, DespesaRequest req)
    {
        var despesa = await db.Despesas
            .FirstOrDefaultAsync(d => d.Id == id && d.IdUsuario == IdUsuario);

        if (despesa is null) return NotFound();

        var erro = await ValidarRefs(req);
        if (erro is not null) return erro;

        despesa.IdCategoria       = req.IdCategoria;
        despesa.IdEstabelecimento = req.IdEstabelecimento;
        despesa.IdProduto         = req.IdProduto;
        despesa.Descricao         = req.Descricao;
        despesa.Valor             = req.Valor;
        if (req.DataCriacao.HasValue)
            despesa.DataCriacao = req.DataCriacao.Value;

        await db.SaveChangesAsync();

        return Ok(await CarregarResponse(id));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var despesa = await db.Despesas
            .FirstOrDefaultAsync(d => d.Id == id && d.IdUsuario == IdUsuario);

        if (despesa is null) return NotFound();

        db.Despesas.Remove(despesa);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<ActionResult?> ValidarRefs(DespesaRequest req)
    {
        if (!await db.Categorias.AnyAsync(c => c.Id == req.IdCategoria))
            return UnprocessableEntity(new { mensagem = "Categoria não encontrada." });

        if (!await db.Estabelecimentos.AnyAsync(e => e.Id == req.IdEstabelecimento))
            return UnprocessableEntity(new { mensagem = "Estabelecimento não encontrado." });

        if (!await db.Produtos.AnyAsync(p => p.Id == req.IdProduto))
            return UnprocessableEntity(new { mensagem = "Produto não encontrado." });

        return null;
    }

    private async Task<DespesaResponse> CarregarResponse(int id) =>
        await db.Despesas
            .Where(d => d.Id == id)
            .Select(d => new DespesaResponse
            {
                Id                  = d.Id,
                IdCategoria         = d.IdCategoria,
                NomeCategoria       = d.Categoria.Nome,
                IdEstabelecimento   = d.IdEstabelecimento,
                NomeEstabelecimento = d.Estabelecimento.Nome,
                IdProduto           = d.IdProduto,
                NomeProduto         = d.Produto.Nome,
                IdMarca             = d.Produto.IdMarca,
                NomeMarca           = d.Produto.Marca.Nome,
                Descricao           = d.Descricao,
                Valor               = d.Valor,
                DataCriacao         = d.DataCriacao
            })
            .FirstAsync();
}
