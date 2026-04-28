using Microsoft.EntityFrameworkCore;
using MinhasFinancas.Api.Models;

namespace MinhasFinancas.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Estabelecimento> Estabelecimentos => Set<Estabelecimento>();
    public DbSet<Marca>         Marcas        => Set<Marca>();
    public DbSet<LinhaProduto>  LinhasProduto => Set<LinhaProduto>();
    public DbSet<Produto>       Produtos      => Set<Produto>();
    public DbSet<Despesa> Despesas => Set<Despesa>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.DataCriacao).HasDefaultValueSql("datetime('now')");
        });

        model.Entity<Categoria>(e =>
        {
            e.HasIndex(c => c.Nome).IsUnique();
            e.HasData(
                new Categoria { Id = 1, Nome = "Essencial" },
                new Categoria { Id = 2, Nome = "Lazer" },
                new Categoria { Id = 3, Nome = "Investimento" }
            );
        });

        model.Entity<Estabelecimento>(e =>
        {
            e.HasIndex(est => est.Nome).IsUnique();
        });

        model.Entity<Marca>(e =>
        {
            e.HasIndex(m => m.Nome).IsUnique();
        });

        model.Entity<LinhaProduto>(e =>
        {
            e.HasIndex(lp => new { lp.IdMarca, lp.Nome }).IsUnique();

            e.HasOne(lp => lp.Marca)
             .WithMany(m => m.LinhasProduto)
             .HasForeignKey(lp => lp.IdMarca);
        });

        model.Entity<Produto>(e =>
        {
            e.HasOne(p => p.Marca)
             .WithMany(m => m.Produtos)
             .HasForeignKey(p => p.IdMarca)
             .IsRequired(false);

            e.HasOne(p => p.LinhaProduto)
             .WithMany(lp => lp.Produtos)
             .HasForeignKey(p => p.IdLinhaProduto)
             .IsRequired(false);
        });

        model.Entity<Despesa>(e =>
        {
            e.Property(d => d.Valor).HasColumnType("REAL");
            e.Property(d => d.DataCriacao).HasDefaultValueSql("datetime('now')");

            e.HasOne(d => d.Usuario)
             .WithMany(u => u.Despesas)
             .HasForeignKey(d => d.IdUsuario);

            e.HasOne(d => d.Categoria)
             .WithMany(c => c.Despesas)
             .HasForeignKey(d => d.IdCategoria);

            e.HasOne(d => d.Estabelecimento)
             .WithMany(est => est.Despesas)
             .HasForeignKey(d => d.IdEstabelecimento);

            e.HasOne(d => d.Produto)
             .WithMany(p => p.Despesas)
             .HasForeignKey(d => d.IdProduto);

            e.HasIndex(d => d.IdUsuario);
            e.HasIndex(d => d.IdCategoria);
            e.HasIndex(d => d.IdEstabelecimento);
            e.HasIndex(d => d.IdProduto);
            e.HasIndex(d => d.DataCriacao);
        });
    }
}
