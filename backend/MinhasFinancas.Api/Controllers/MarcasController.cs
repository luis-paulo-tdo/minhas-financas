using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Api.Data;
using MinhasFinancas.Api.DTOs;
using MinhasFinancas.Api.Models;

namespace MinhasFinancas.Api.Controllers;

[ApiController]
[Route("api/marcas")]
[Authorize]
public class MarcasController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MarcaResponse>>> Listar([FromQuery] string? busca)
    {
        var query = db.Marcas.AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(m => m.Nome.Contains(busca));

        var lista = await query
            .OrderBy(m => m.Nome)
            .Select(m => new MarcaResponse { Id = m.Id, Nome = m.Nome })
            .ToListAsync();

        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MarcaResponse>> Obter(int id)
    {
        var marca = await db.Marcas.FindAsync(id);

        if (marca is null) return NotFound();

        return Ok(new MarcaResponse { Id = marca.Id, Nome = marca.Nome });
    }

    [HttpPost]
    public async Task<ActionResult<MarcaResponse>> Criar(MarcaRequest req)
    {
        if (await db.Marcas.AnyAsync(m => m.Nome == req.Nome))
            return Conflict(new { mensagem = "Já existe uma marca com este nome." });

        var marca = new Marca { Nome = req.Nome };
        db.Marcas.Add(marca);
        await db.SaveChangesAsync();

        var response = new MarcaResponse { Id = marca.Id, Nome = marca.Nome };
        return CreatedAtAction(nameof(Obter), new { id = marca.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MarcaResponse>> Atualizar(int id, MarcaRequest req)
    {
        var marca = await db.Marcas.FindAsync(id);

        if (marca is null) return NotFound();

        if (await db.Marcas.AnyAsync(m => m.Nome == req.Nome && m.Id != id))
            return Conflict(new { mensagem = "Já existe uma marca com este nome." });

        marca.Nome = req.Nome;
        await db.SaveChangesAsync();

        return Ok(new MarcaResponse { Id = marca.Id, Nome = marca.Nome });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var marca = await db.Marcas.FindAsync(id);

        if (marca is null) return NotFound();

        db.Marcas.Remove(marca);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
