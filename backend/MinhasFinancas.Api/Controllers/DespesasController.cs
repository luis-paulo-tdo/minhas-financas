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
                NomeProduto         = d.Produto != null ? d.Produto.Nome : null,
                IdMarca             = d.Produto != null ? d.Produto.IdMarca : (int?)null,
                NomeMarca           = d.Produto != null ? d.Produto.Marca.Nome : null,
                IdLinhaProduto      = d.Produto != null ? d.Produto.IdLinhaProduto : (int?)null,
                NomeLinhaProduto    = d.Produto != null ? d.Produto.LinhaProduto.Nome : null,
                Descricao           = d.Descricao,
                Valor               = d.Valor,
                PrecoGranel         = d.PrecoGranel,
                UnidadeGranel       = d.UnidadeGranel,
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

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardResponse>> Dashboard(
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

        var despesas = await query
            .Select(d => new
            {
                d.Valor,
                NomeCategoria       = d.Categoria.Nome,
                NomeEstabelecimento = d.Estabelecimento.Nome,
                NomeProduto         = d.Produto != null ? d.Produto.Nome         : null,
                NomeMarca           = d.Produto != null ? d.Produto.Marca.Nome   : null,
                d.DataCriacao
            })
            .ToListAsync();

        var resumo = new ResumoDespesas
        {
            TotalEssencial    = despesas.Where(d => d.NomeCategoria == "Essencial").Sum(d => d.Valor),
            TotalLazer        = despesas.Where(d => d.NomeCategoria == "Lazer").Sum(d => d.Valor),
            TotalInvestimento = despesas.Where(d => d.NomeCategoria == "Investimento").Sum(d => d.Valor),
        };
        resumo.TotalGeral = resumo.TotalEssencial + resumo.TotalLazer + resumo.TotalInvestimento;

        var evolucao = despesas
            .GroupBy(d => new { d.DataCriacao.Year, d.DataCriacao.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new EvolucaoMensalItem
            {
                Ano               = g.Key.Year,
                Mes               = g.Key.Month,
                TotalEssencial    = g.Where(d => d.NomeCategoria == "Essencial").Sum(d => d.Valor),
                TotalLazer        = g.Where(d => d.NomeCategoria == "Lazer").Sum(d => d.Valor),
                TotalInvestimento = g.Where(d => d.NomeCategoria == "Investimento").Sum(d => d.Valor),
            })
            .ToList();

        var topEstabelecimentos = despesas
            .GroupBy(d => d.NomeEstabelecimento)
            .Select(g => new RankingItem { Nome = g.Key, Total = g.Sum(d => d.Valor) })
            .OrderByDescending(r => r.Total)
            .Take(5)
            .ToList();

        var topProdutos = despesas
            .Where(d => d.NomeProduto != null)
            .GroupBy(d => new { d.NomeProduto, d.NomeMarca })
            .Select(g => new RankingItem
            {
                Nome    = g.Key.NomeProduto!,
                Detalhe = g.Key.NomeMarca,
                Total   = g.Sum(d => d.Valor)
            })
            .OrderByDescending(r => r.Total)
            .Take(5)
            .ToList();

        return Ok(new DashboardResponse
        {
            Resumo              = resumo,
            EvolucaoMensal      = evolucao,
            TopEstabelecimentos = topEstabelecimentos,
            TopProdutos         = topProdutos
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
                NomeProduto         = d.Produto != null ? d.Produto.Nome : null,
                IdMarca             = d.Produto != null ? d.Produto.IdMarca : (int?)null,
                NomeMarca           = d.Produto != null ? d.Produto.Marca.Nome : null,
                IdLinhaProduto      = d.Produto != null ? d.Produto.IdLinhaProduto : (int?)null,
                NomeLinhaProduto    = d.Produto != null ? d.Produto.LinhaProduto.Nome : null,
                Descricao           = d.Descricao,
                Valor               = d.Valor,
                PrecoGranel         = d.PrecoGranel,
                UnidadeGranel       = d.UnidadeGranel,
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
            PrecoGranel       = req.PrecoGranel,
            UnidadeGranel     = req.UnidadeGranel,
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
        despesa.PrecoGranel       = req.PrecoGranel;
        despesa.UnidadeGranel     = req.UnidadeGranel;
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

        if (req.IdProduto.HasValue && !await db.Produtos.AnyAsync(p => p.Id == req.IdProduto.Value))
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
                NomeProduto         = d.Produto != null ? d.Produto.Nome : null,
                IdMarca             = d.Produto != null ? d.Produto.IdMarca : (int?)null,
                NomeMarca           = d.Produto != null ? d.Produto.Marca.Nome : null,
                IdLinhaProduto      = d.Produto != null ? d.Produto.IdLinhaProduto : (int?)null,
                NomeLinhaProduto    = d.Produto != null ? d.Produto.LinhaProduto.Nome : null,
                Descricao           = d.Descricao,
                Valor               = d.Valor,
                PrecoGranel         = d.PrecoGranel,
                UnidadeGranel       = d.UnidadeGranel,
                DataCriacao         = d.DataCriacao
            })
            .FirstAsync();
}
