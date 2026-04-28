using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Api.Data;
using MinhasFinancas.Api.DTOs;
using MinhasFinancas.Api.Models;

namespace MinhasFinancas.Api.Controllers;

[ApiController]
[Route("api/produtos")]
[Authorize]
public class ProdutosController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProdutoResponse>>> Listar(
        [FromQuery] string? busca,
        [FromQuery] int?    idMarca,
        [FromQuery] int?    idLinhaProduto)
    {
        var query = db.Produtos
            .Include(p => p.Marca)
            .Include(p => p.LinhaProduto)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(p => p.Nome.Contains(busca));

        if (idMarca.HasValue)
            query = query.Where(p => p.IdMarca == idMarca.Value);

        if (idLinhaProduto.HasValue)
            query = query.Where(p => p.IdLinhaProduto == idLinhaProduto.Value);

        var lista = await query
            .OrderBy(p => p.Nome)
            .Select(p => new ProdutoResponse
            {
                Id               = p.Id,
                Nome             = p.Nome,
                IdMarca          = p.IdMarca,
                NomeMarca        = p.Marca != null ? p.Marca.Nome : null,
                IdLinhaProduto   = p.IdLinhaProduto,
                NomeLinhaProduto = p.LinhaProduto != null ? p.LinhaProduto.Nome : null
            })
            .ToListAsync();

        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProdutoResponse>> Obter(int id)
    {
        var produto = await db.Produtos
            .Include(p => p.Marca)
            .Include(p => p.LinhaProduto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produto is null) return NotFound();

        return Ok(ToResponse(produto));
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoResponse>> Criar(ProdutoRequest req)
    {
        var erro = await ValidarRefs(req);
        if (erro is not null) return erro;

        if (await db.Produtos.AnyAsync(p => p.Nome == req.Nome && p.IdMarca == req.IdMarca))
            return Conflict(new { mensagem = "Já existe um produto com este nome para a marca informada." });

        var produto = new Produto { Nome = req.Nome, IdMarca = req.IdMarca, IdLinhaProduto = req.IdLinhaProduto };
        db.Produtos.Add(produto);
        await db.SaveChangesAsync();

        if (produto.IdMarca.HasValue)
            await db.Entry(produto).Reference(p => p.Marca).LoadAsync();
        if (produto.IdLinhaProduto.HasValue)
            await db.Entry(produto).Reference(p => p.LinhaProduto).LoadAsync();

        return CreatedAtAction(nameof(Obter), new { id = produto.Id }, ToResponse(produto));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProdutoResponse>> Atualizar(int id, ProdutoRequest req)
    {
        var produto = await db.Produtos
            .Include(p => p.Marca)
            .Include(p => p.LinhaProduto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produto is null) return NotFound();

        var erro = await ValidarRefs(req);
        if (erro is not null) return erro;

        if (await db.Produtos.AnyAsync(p => p.Nome == req.Nome && p.IdMarca == req.IdMarca && p.Id != id))
            return Conflict(new { mensagem = "Já existe um produto com este nome para a marca informada." });

        produto.Nome           = req.Nome;
        produto.IdMarca        = req.IdMarca;
        produto.IdLinhaProduto = req.IdLinhaProduto;
        await db.SaveChangesAsync();

        if (produto.IdMarca.HasValue)
            await db.Entry(produto).Reference(p => p.Marca).LoadAsync();
        else
            produto.Marca = null;

        if (produto.IdLinhaProduto.HasValue)
            await db.Entry(produto).Reference(p => p.LinhaProduto).LoadAsync();
        else
            produto.LinhaProduto = null;

        return Ok(ToResponse(produto));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var produto = await db.Produtos.FindAsync(id);

        if (produto is null) return NotFound();

        db.Produtos.Remove(produto);
        await db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<ActionResult?> ValidarRefs(ProdutoRequest req)
    {
        if (req.IdMarca.HasValue && !await db.Marcas.AnyAsync(m => m.Id == req.IdMarca.Value))
            return UnprocessableEntity(new { mensagem = "Marca não encontrada." });

        if (req.IdLinhaProduto.HasValue)
        {
            var linha = await db.LinhasProduto.FirstOrDefaultAsync(lp => lp.Id == req.IdLinhaProduto.Value);
            if (linha is null)
                return UnprocessableEntity(new { mensagem = "Linha de produto não encontrada." });
            if (req.IdMarca.HasValue && linha.IdMarca != req.IdMarca.Value)
                return UnprocessableEntity(new { mensagem = "A linha de produto não pertence à marca informada." });
        }

        return null;
    }

    private static ProdutoResponse ToResponse(Produto p) => new()
    {
        Id               = p.Id,
        Nome             = p.Nome,
        IdMarca          = p.IdMarca,
        NomeMarca        = p.Marca?.Nome,
        IdLinhaProduto   = p.IdLinhaProduto,
        NomeLinhaProduto = p.LinhaProduto?.Nome
    };
}
