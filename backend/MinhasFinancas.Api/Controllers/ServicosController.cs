using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Api.Data;
using MinhasFinancas.Api.DTOs;
using MinhasFinancas.Api.Models;

namespace MinhasFinancas.Api.Controllers;

[ApiController]
[Route("api/servicos")]
[Authorize]
public class ServicosController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServicoResponse>>> Listar([FromQuery] string? busca)
    {
        var query = db.Servicos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(s => s.Nome.Contains(busca));

        var lista = await query
            .OrderBy(s => s.Nome)
            .Select(s => new ServicoResponse { Id = s.Id, Nome = s.Nome })
            .ToListAsync();

        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServicoResponse>> Obter(int id)
    {
        var servico = await db.Servicos.FindAsync(id);

        if (servico is null) return NotFound();

        return Ok(new ServicoResponse { Id = servico.Id, Nome = servico.Nome });
    }

    [HttpPost]
    public async Task<ActionResult<ServicoResponse>> Criar(ServicoRequest req)
    {
        if (await db.Servicos.AnyAsync(s => s.Nome == req.Nome))
            return Conflict(new { mensagem = "Já existe um serviço com este nome." });

        var servico = new Servico { Nome = req.Nome };
        db.Servicos.Add(servico);
        await db.SaveChangesAsync();

        var response = new ServicoResponse { Id = servico.Id, Nome = servico.Nome };
        return CreatedAtAction(nameof(Obter), new { id = servico.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ServicoResponse>> Atualizar(int id, ServicoRequest req)
    {
        var servico = await db.Servicos.FindAsync(id);

        if (servico is null) return NotFound();

        if (await db.Servicos.AnyAsync(s => s.Nome == req.Nome && s.Id != id))
            return Conflict(new { mensagem = "Já existe um serviço com este nome." });

        servico.Nome = req.Nome;
        await db.SaveChangesAsync();

        return Ok(new ServicoResponse { Id = servico.Id, Nome = servico.Nome });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var servico = await db.Servicos.FindAsync(id);

        if (servico is null) return NotFound();

        db.Servicos.Remove(servico);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
