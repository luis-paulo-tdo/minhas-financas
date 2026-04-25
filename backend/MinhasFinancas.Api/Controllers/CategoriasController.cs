using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Api.Data;
using MinhasFinancas.Api.DTOs;

namespace MinhasFinancas.Api.Controllers;

[ApiController]
[Route("api/categorias")]
[Authorize]
public class CategoriasController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaResponse>>> Listar()
    {
        var categorias = await db.Categorias
            .OrderBy(c => c.Id)
            .Select(c => new CategoriaResponse { Id = c.Id, Nome = c.Nome })
            .ToListAsync();

        return Ok(categorias);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoriaResponse>> Obter(int id)
    {
        var categoria = await db.Categorias.FindAsync(id);

        if (categoria is null) return NotFound();

        return Ok(new CategoriaResponse { Id = categoria.Id, Nome = categoria.Nome });
    }
}
