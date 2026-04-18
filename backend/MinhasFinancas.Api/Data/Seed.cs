using MinhasFinancas.Api.Models;

namespace MinhasFinancas.Api.Data;

public static class Seed
{
    public static void Executar(AppDbContext db)
    {
        if (db.Usuarios.Any()) return;

        var marcas = new List<Marca>
        {
            new() { Nome = "Nestlé" },
            new() { Nome = "Camil" },
            new() { Nome = "Friboi" },
            new() { Nome = "Sadia" },
            new() { Nome = "Coca-Cola" },
            new() { Nome = "Nike" },
            new() { Nome = "Samsung" },
            new() { Nome = "Apple" },
            new() { Nome = "Petrobras" },
            new() { Nome = "Genérico" },
        };
        db.Marcas.AddRange(marcas);
        db.SaveChanges();

        var produtos = new List<Produto>
        {
            new() { Nome = "Arroz 5kg",           IdMarca = marcas[1].Id },
            new() { Nome = "Feijão 1kg",           IdMarca = marcas[1].Id },
            new() { Nome = "Frango Inteiro",       IdMarca = marcas[2].Id },
            new() { Nome = "Linguiça Toscana",     IdMarca = marcas[3].Id },
            new() { Nome = "Nescafé 500g",         IdMarca = marcas[0].Id },
            new() { Nome = "Refrigerante 2L",      IdMarca = marcas[4].Id },
            new() { Nome = "Tênis Running",        IdMarca = marcas[5].Id },
            new() { Nome = "Smartphone Galaxy",    IdMarca = marcas[6].Id },
            new() { Nome = "iPhone 15",            IdMarca = marcas[7].Id },
            new() { Nome = "Gasolina Comum",       IdMarca = marcas[8].Id },
            new() { Nome = "Consulta Médica",      IdMarca = marcas[9].Id },
            new() { Nome = "Ingresso Cinema",      IdMarca = marcas[9].Id },
            new() { Nome = "Tesouro Direto",       IdMarca = marcas[9].Id },
            new() { Nome = "Energia Elétrica",     IdMarca = marcas[9].Id },
            new() { Nome = "Internet Fibra",       IdMarca = marcas[9].Id },
        };
        db.Produtos.AddRange(produtos);
        db.SaveChanges();

        var estabelecimentos = new List<Estabelecimento>
        {
            new() { Nome = "Supermercado Extra" },
            new() { Nome = "Carrefour" },
            new() { Nome = "Posto Shell" },
            new() { Nome = "Farmácia Drogasil" },
            new() { Nome = "Shopping Iguatemi" },
            new() { Nome = "Cinema Cinemark" },
            new() { Nome = "Clínica São Lucas" },
            new() { Nome = "Corretora XP" },
            new() { Nome = "Enel Distribuição" },
            new() { Nome = "Vivo Fibra" },
        };
        db.Estabelecimentos.AddRange(estabelecimentos);
        db.SaveChanges();

        var usuario = new Usuario
        {
            Nome  = "Luis Paulo",
            Email = "luis.paulo.tdo@gmail.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("123456"),
        };
        db.Usuarios.Add(usuario);
        db.SaveChanges();

        var idEssencial    = db.Categorias.First(c => c.Nome == "Essencial").Id;
        var idLazer        = db.Categorias.First(c => c.Nome == "Lazer").Id;
        var idInvestimento = db.Categorias.First(c => c.Nome == "Investimento").Id;

        var despesas = new List<Despesa>
        {
            new() { IdUsuario = usuario.Id, IdCategoria = idEssencial,    IdEstabelecimento = estabelecimentos[0].Id, IdProduto = produtos[0].Id,  Valor = 28.90m,   DataCriacao = Ago(90), Descricao = null },
            new() { IdUsuario = usuario.Id, IdCategoria = idEssencial,    IdEstabelecimento = estabelecimentos[0].Id, IdProduto = produtos[1].Id,  Valor = 9.50m,    DataCriacao = Ago(89) },
            new() { IdUsuario = usuario.Id, IdCategoria = idEssencial,    IdEstabelecimento = estabelecimentos[0].Id, IdProduto = produtos[2].Id,  Valor = 35.00m,   DataCriacao = Ago(88) },
            new() { IdUsuario = usuario.Id, IdCategoria = idEssencial,    IdEstabelecimento = estabelecimentos[1].Id, IdProduto = produtos[3].Id,  Valor = 22.00m,   DataCriacao = Ago(85) },
            new() { IdUsuario = usuario.Id, IdCategoria = idEssencial,    IdEstabelecimento = estabelecimentos[1].Id, IdProduto = produtos[4].Id,  Valor = 18.90m,   DataCriacao = Ago(80) },
            new() { IdUsuario = usuario.Id, IdCategoria = idEssencial,    IdEstabelecimento = estabelecimentos[2].Id, IdProduto = produtos[9].Id,  Valor = 180.00m,  DataCriacao = Ago(75) },
            new() { IdUsuario = usuario.Id, IdCategoria = idEssencial,    IdEstabelecimento = estabelecimentos[3].Id, IdProduto = produtos[10].Id, Valor = 250.00m,  DataCriacao = Ago(70), Descricao = "Consulta cardiologista" },
            new() { IdUsuario = usuario.Id, IdCategoria = idEssencial,    IdEstabelecimento = estabelecimentos[8].Id, IdProduto = produtos[13].Id, Valor = 210.00m,  DataCriacao = Ago(60) },
            new() { IdUsuario = usuario.Id, IdCategoria = idEssencial,    IdEstabelecimento = estabelecimentos[9].Id, IdProduto = produtos[14].Id, Valor = 129.90m,  DataCriacao = Ago(58) },
            new() { IdUsuario = usuario.Id, IdCategoria = idEssencial,    IdEstabelecimento = estabelecimentos[0].Id, IdProduto = produtos[5].Id,  Valor = 12.00m,   DataCriacao = Ago(55) },
            new() { IdUsuario = usuario.Id, IdCategoria = idLazer,        IdEstabelecimento = estabelecimentos[5].Id, IdProduto = produtos[11].Id, Valor = 34.00m,   DataCriacao = Ago(50), Descricao = "Filme em família" },
            new() { IdUsuario = usuario.Id, IdCategoria = idLazer,        IdEstabelecimento = estabelecimentos[4].Id, IdProduto = produtos[6].Id,  Valor = 399.90m,  DataCriacao = Ago(45) },
            new() { IdUsuario = usuario.Id, IdCategoria = idLazer,        IdEstabelecimento = estabelecimentos[5].Id, IdProduto = produtos[11].Id, Valor = 34.00m,   DataCriacao = Ago(40) },
            new() { IdUsuario = usuario.Id, IdCategoria = idLazer,        IdEstabelecimento = estabelecimentos[4].Id, IdProduto = produtos[5].Id,  Valor = 24.00m,   DataCriacao = Ago(35) },
            new() { IdUsuario = usuario.Id, IdCategoria = idInvestimento, IdEstabelecimento = estabelecimentos[7].Id, IdProduto = produtos[12].Id, Valor = 500.00m,  DataCriacao = Ago(30), Descricao = "Aporte mensal" },
            new() { IdUsuario = usuario.Id, IdCategoria = idInvestimento, IdEstabelecimento = estabelecimentos[7].Id, IdProduto = produtos[12].Id, Valor = 500.00m,  DataCriacao = Ago(2),  Descricao = "Aporte mensal" },
            new() { IdUsuario = usuario.Id, IdCategoria = idEssencial,    IdEstabelecimento = estabelecimentos[1].Id, IdProduto = produtos[0].Id,  Valor = 29.50m,   DataCriacao = Ago(28) },
            new() { IdUsuario = usuario.Id, IdCategoria = idEssencial,    IdEstabelecimento = estabelecimentos[2].Id, IdProduto = produtos[9].Id,  Valor = 195.00m,  DataCriacao = Ago(20) },
            new() { IdUsuario = usuario.Id, IdCategoria = idLazer,        IdEstabelecimento = estabelecimentos[4].Id, IdProduto = produtos[6].Id,  Valor = 219.90m,  DataCriacao = Ago(15) },
            new() { IdUsuario = usuario.Id, IdCategoria = idEssencial,    IdEstabelecimento = estabelecimentos[3].Id, IdProduto = produtos[10].Id, Valor = 80.00m,   DataCriacao = Ago(10), Descricao = "Exame de sangue" },
            new() { IdUsuario = usuario.Id, IdCategoria = idEssencial,    IdEstabelecimento = estabelecimentos[0].Id, IdProduto = produtos[2].Id,  Valor = 38.50m,   DataCriacao = Ago(7) },
            new() { IdUsuario = usuario.Id, IdCategoria = idLazer,        IdEstabelecimento = estabelecimentos[5].Id, IdProduto = produtos[11].Id, Valor = 34.00m,   DataCriacao = Ago(5) },
            new() { IdUsuario = usuario.Id, IdCategoria = idInvestimento, IdEstabelecimento = estabelecimentos[7].Id, IdProduto = produtos[12].Id, Valor = 1000.00m, DataCriacao = Ago(3), Descricao = "Aporte extra" },
            new() { IdUsuario = usuario.Id, IdCategoria = idEssencial,    IdEstabelecimento = estabelecimentos[8].Id, IdProduto = produtos[13].Id, Valor = 210.00m,  DataCriacao = Ago(1) },
        };
        db.Despesas.AddRange(despesas);
        db.SaveChanges();
    }

    private static DateTime Ago(int dias) => DateTime.UtcNow.AddDays(-dias);
}
