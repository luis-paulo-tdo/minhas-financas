using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Api.Data;
using MinhasFinancas.Api.DTOs;
using MinhasFinancas.Api.Models;

namespace MinhasFinancas.Api.Controllers;

[ApiController]
[Route("api/linhas-produto")]
[Authorize]
public class LinhasProdutoController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LinhaProdutoResponse>>> Listar(
        [FromQuery] int?    idMarca,
        [FromQuery] string? busca)
    {
        var query = db.LinhasProduto.Include(lp => lp.Marca).AsQueryable();

        if (idMarca.HasValue)
            query = query.Where(lp => lp.IdMarca == idMarca.Value);

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(lp => lp.Nome.Contains(busca));

        var lista = await query
            .OrderBy(lp => lp.Marca.Nome).ThenBy(lp => lp.Nome)
            .Select(lp => new LinhaProdutoResponse
            {
                Id        = lp.Id,
                Nome      = lp.Nome,
                IdMarca   = lp.IdMarca,
                NomeMarca = lp.Marca.Nome
            })
            .ToListAsync();

        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LinhaProdutoResponse>> Obter(int id)
    {
        var linha = await db.LinhasProduto
            .Include(lp => lp.Marca)
            .FirstOrDefaultAsync(lp => lp.Id == id);

        if (linha is null) return NotFound();

        return Ok(new LinhaProdutoResponse
        {
            Id        = linha.Id,
            Nome      = linha.Nome,
            IdMarca   = linha.IdMarca,
            NomeMarca = linha.Marca.Nome
        });
    }

    [HttpPost]
    public async Task<ActionResult<LinhaProdutoResponse>> Criar(LinhaProdutoRequest req)
    {
        if (!await db.Marcas.AnyAsync(m => m.Id == req.IdMarca))
            return UnprocessableEntity(new { mensagem = "Marca não encontrada." });

        if (await db.LinhasProduto.AnyAsync(lp => lp.Nome == req.Nome && lp.IdMarca == req.IdMarca))
            return Conflict(new { mensagem = "Já existe uma linha de produto com este nome para a marca informada." });

        var linha = new LinhaProduto { Nome = req.Nome, IdMarca = req.IdMarca };
        db.LinhasProduto.Add(linha);
        await db.SaveChangesAsync();

        await db.Entry(linha).Reference(lp => lp.Marca).LoadAsync();

        return CreatedAtAction(nameof(Obter), new { id = linha.Id }, new LinhaProdutoResponse
        {
            Id        = linha.Id,
            Nome      = linha.Nome,
            IdMarca   = linha.IdMarca,
            NomeMarca = linha.Marca.Nome
        });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<LinhaProdutoResponse>> Atualizar(int id, LinhaProdutoRequest req)
    {
        var linha = await db.LinhasProduto
            .Include(lp => lp.Marca)
            .FirstOrDefaultAsync(lp => lp.Id == id);

        if (linha is null) return NotFound();

        if (!await db.Marcas.AnyAsync(m => m.Id == req.IdMarca))
            return UnprocessableEntity(new { mensagem = "Marca não encontrada." });

        if (await db.LinhasProduto.AnyAsync(lp => lp.Nome == req.Nome && lp.IdMarca == req.IdMarca && lp.Id != id))
            return Conflict(new { mensagem = "Já existe uma linha de produto com este nome para a marca informada." });

        linha.Nome    = req.Nome;
        linha.IdMarca = req.IdMarca;
        await db.SaveChangesAsync();

        await db.Entry(linha).Reference(lp => lp.Marca).LoadAsync();

        return Ok(new LinhaProdutoResponse
        {
            Id        = linha.Id,
            Nome      = linha.Nome,
            IdMarca   = linha.IdMarca,
            NomeMarca = linha.Marca.Nome
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var linha = await db.LinhasProduto.FindAsync(id);

        if (linha is null) return NotFound();

        db.LinhasProduto.Remove(linha);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
