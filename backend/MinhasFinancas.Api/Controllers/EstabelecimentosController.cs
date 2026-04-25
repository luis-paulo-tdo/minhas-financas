using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Api.Data;
using MinhasFinancas.Api.DTOs;
using MinhasFinancas.Api.Models;

namespace MinhasFinancas.Api.Controllers;

[ApiController]
[Route("api/estabelecimentos")]
[Authorize]
public class EstabelecimentosController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EstabelecimentoResponse>>> Listar([FromQuery] string? busca)
    {
        var query = db.Estabelecimentos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(e => e.Nome.Contains(busca));

        var lista = await query
            .OrderBy(e => e.Nome)
            .Select(e => new EstabelecimentoResponse { Id = e.Id, Nome = e.Nome })
            .ToListAsync();

        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EstabelecimentoResponse>> Obter(int id)
    {
        var estabelecimento = await db.Estabelecimentos.FindAsync(id);

        if (estabelecimento is null) return NotFound();

        return Ok(new EstabelecimentoResponse { Id = estabelecimento.Id, Nome = estabelecimento.Nome });
    }

    [HttpPost]
    public async Task<ActionResult<EstabelecimentoResponse>> Criar(EstabelecimentoRequest req)
    {
        if (await db.Estabelecimentos.AnyAsync(e => e.Nome == req.Nome))
            return Conflict(new { mensagem = "Já existe um estabelecimento com este nome." });

        var estabelecimento = new Estabelecimento { Nome = req.Nome };
        db.Estabelecimentos.Add(estabelecimento);
        await db.SaveChangesAsync();

        var response = new EstabelecimentoResponse { Id = estabelecimento.Id, Nome = estabelecimento.Nome };
        return CreatedAtAction(nameof(Obter), new { id = estabelecimento.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EstabelecimentoResponse>> Atualizar(int id, EstabelecimentoRequest req)
    {
        var estabelecimento = await db.Estabelecimentos.FindAsync(id);

        if (estabelecimento is null) return NotFound();

        if (await db.Estabelecimentos.AnyAsync(e => e.Nome == req.Nome && e.Id != id))
            return Conflict(new { mensagem = "Já existe um estabelecimento com este nome." });

        estabelecimento.Nome = req.Nome;
        await db.SaveChangesAsync();

        return Ok(new EstabelecimentoResponse { Id = estabelecimento.Id, Nome = estabelecimento.Nome });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var estabelecimento = await db.Estabelecimentos.FindAsync(id);

        if (estabelecimento is null) return NotFound();

        db.Estabelecimentos.Remove(estabelecimento);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
