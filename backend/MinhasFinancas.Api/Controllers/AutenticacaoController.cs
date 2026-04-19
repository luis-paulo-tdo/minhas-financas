using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MinhasFinancas.Api.Data;
using MinhasFinancas.Api.DTOs;
using MinhasFinancas.Api.Models;

namespace MinhasFinancas.Api.Controllers;

[ApiController]
[Route("api/autenticacao")]
public class AutenticacaoController(AppDbContext db, IConfiguration config) : ControllerBase
{
    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar(RegistroRequest req)
    {
        if (await db.Usuarios.AnyAsync(u => u.Email == req.Email))
            return Conflict(new { mensagem = "E-mail já cadastrado." });

        var usuario = new Usuario
        {
            Nome      = req.Nome,
            Email     = req.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(req.Senha)
        };

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        return Created(string.Empty, new { mensagem = "Usuário criado com sucesso." });
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest req)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == req.Email);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(req.Senha, usuario.SenhaHash))
            return Unauthorized(new { mensagem = "E-mail ou senha inválidos." });

        var token = GerarToken(usuario);

        return Ok(token);
    }

    private LoginResponse GerarToken(Usuario usuario)
    {
        var jwt      = config.GetSection("Jwt");
        var chave    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Chave"]!));
        var dataExpiracao = DateTime.UtcNow.AddHours(int.Parse(jwt["ExpiracaoHoras"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Name,  usuario.Nome),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             jwt["Emissor"],
            audience:           jwt["Audiencia"],
            claims:             claims,
            expires:            dataExpiracao,
            signingCredentials: new SigningCredentials(chave, SecurityAlgorithms.HmacSha256)
        );

        return new LoginResponse
        {
            Token    = new JwtSecurityTokenHandler().WriteToken(token),
            DataExpiracao = dataExpiracao,
            Nome     = usuario.Nome,
            Email    = usuario.Email
        };
    }
}
