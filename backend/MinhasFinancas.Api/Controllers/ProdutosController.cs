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
        [FromQuery] int? idMarca)
    {
        var query = db.Produtos.Include(p => p.Marca).AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(p => p.Nome.Contains(busca));

        if (idMarca.HasValue)
            query = query.Where(p => p.IdMarca == idMarca.Value);

        var lista = await query
            .OrderBy(p => p.Nome)
            .Select(p => new ProdutoResponse
            {
                Id        = p.Id,
                Nome      = p.Nome,
                IdMarca   = p.IdMarca,
                NomeMarca = p.Marca.Nome
            })
            .ToListAsync();

        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProdutoResponse>> Obter(int id)
    {
        var produto = await db.Produtos
            .Include(p => p.Marca)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produto is null) return NotFound();

        return Ok(new ProdutoResponse
        {
            Id        = produto.Id,
            Nome      = produto.Nome,
            IdMarca   = produto.IdMarca,
            NomeMarca = produto.Marca.Nome
        });
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoResponse>> Criar(ProdutoRequest req)
    {
        if (!await db.Marcas.AnyAsync(m => m.Id == req.IdMarca))
            return UnprocessableEntity(new { mensagem = "Marca não encontrada." });

        if (await db.Produtos.AnyAsync(p => p.Nome == req.Nome && p.IdMarca == req.IdMarca))
            return Conflict(new { mensagem = "Já existe um produto com este nome para a marca informada." });

        var produto = new Produto { Nome = req.Nome, IdMarca = req.IdMarca };
        db.Produtos.Add(produto);
        await db.SaveChangesAsync();

        await db.Entry(produto).Reference(p => p.Marca).LoadAsync();

        var response = new ProdutoResponse
        {
            Id        = produto.Id,
            Nome      = produto.Nome,
            IdMarca   = produto.IdMarca,
            NomeMarca = produto.Marca.Nome
        };

        return CreatedAtAction(nameof(Obter), new { id = produto.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProdutoResponse>> Atualizar(int id, ProdutoRequest req)
    {
        var produto = await db.Produtos
            .Include(p => p.Marca)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produto is null) return NotFound();

        if (!await db.Marcas.AnyAsync(m => m.Id == req.IdMarca))
            return UnprocessableEntity(new { mensagem = "Marca não encontrada." });

        if (await db.Produtos.AnyAsync(p => p.Nome == req.Nome && p.IdMarca == req.IdMarca && p.Id != id))
            return Conflict(new { mensagem = "Já existe um produto com este nome para a marca informada." });

        produto.Nome    = req.Nome;
        produto.IdMarca = req.IdMarca;
        await db.SaveChangesAsync();

        await db.Entry(produto).Reference(p => p.Marca).LoadAsync();

        return Ok(new ProdutoResponse
        {
            Id        = produto.Id,
            Nome      = produto.Nome,
            IdMarca   = produto.IdMarca,
            NomeMarca = produto.Marca.Nome
        });
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
}
