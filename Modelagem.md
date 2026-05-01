# Modelagem do Banco de Dados

## Diagrama de relacionamentos

```
Usuarios
    │
    └──→ Despesas ──→ Categorias
              │
              ├──→ Estabelecimentos
              │
              └──→ Produtos ──→ Marcas
                       │
                       └──→ LinhasProduto ──→ Marcas
```

---

## Tabelas

### `Usuarios`

| Coluna       | Tipo    | Restrições                          |
|--------------|---------|-------------------------------------|
| `Id`         | INTEGER | PK AUTOINCREMENT                    |
| `Nome`       | TEXT    | NOT NULL                            |
| `Email`      | TEXT    | NOT NULL                            |
| `SenhaHash`  | TEXT    | NOT NULL — bcrypt                   |
| `DataCriacao`| TEXT    | NOT NULL DEFAULT (datetime('now'))  |

**Índices:** `UNIQUE (Email)`

---

### `Categorias`

| Coluna | Tipo    | Restrições       |
|--------|---------|------------------|
| `Id`   | INTEGER | PK AUTOINCREMENT |
| `Nome` | TEXT    | NOT NULL         |

**Índices:** `UNIQUE (Nome)`

**Dados fixos (seed):**
| Id | Nome          |
|----|---------------|
| 1  | Essencial     |
| 2  | Lazer         |
| 3  | Investimento  |

---

### `Estabelecimentos`

| Coluna | Tipo    | Restrições       |
|--------|---------|------------------|
| `Id`   | INTEGER | PK AUTOINCREMENT |
| `Nome` | TEXT    | NOT NULL         |

**Índices:** `UNIQUE (Nome)`

---

### `Marcas`

| Coluna | Tipo    | Restrições       |
|--------|---------|------------------|
| `Id`   | INTEGER | PK AUTOINCREMENT |
| `Nome` | TEXT    | NOT NULL         |

**Índices:** `UNIQUE (Nome)`

---

### `LinhasProduto`

Agrupa produtos de uma mesma marca em linhas (ex: Marca = Nestlé, Linha = Kit Kat).

| Coluna    | Tipo    | Restrições              |
|-----------|---------|-------------------------|
| `Id`      | INTEGER | PK AUTOINCREMENT        |
| `IdMarca` | INTEGER | NOT NULL, FK → Marcas   |
| `Nome`    | TEXT    | NOT NULL                |

**Índices:** `UNIQUE (IdMarca, Nome)`

---

### `Produtos`

| Coluna           | Tipo    | Restrições                     |
|------------------|---------|--------------------------------|
| `Id`             | INTEGER | PK AUTOINCREMENT               |
| `IdMarca`        | INTEGER | NULL, FK → Marcas              |
| `IdLinhaProduto` | INTEGER | NULL, FK → LinhasProduto       |
| `Nome`           | TEXT    | NOT NULL                       |

> Marca e Linha de Produto são opcionais — permite registrar produtos genéricos sem marca definida.

---

### `Despesas`

| Coluna              | Tipo    | Restrições                         |
|---------------------|---------|------------------------------------|
| `Id`                | INTEGER | PK AUTOINCREMENT                   |
| `IdUsuario`         | INTEGER | NOT NULL, FK → Usuarios            |
| `IdCategoria`       | INTEGER | NOT NULL, FK → Categorias          |
| `IdEstabelecimento` | INTEGER | NOT NULL, FK → Estabelecimentos    |
| `IdProduto`         | INTEGER | NULL, FK → Produtos                |
| `Descricao`         | TEXT    | NULL                               |
| `Valor`             | REAL    | NOT NULL                           |
| `PrecoGranel`       | REAL    | NULL — preço por unidade de medida |
| `UnidadeGranel`     | TEXT    | NULL — ex: "kg", "L", "100g"       |
| `DataCriacao`       | TEXT    | NOT NULL DEFAULT (datetime('now')) |

> `PrecoGranel` e `UnidadeGranel` são usados quando o produto é comprado a granel.
> `IdProduto` é opcional — permite registrar despesas sem produto vinculado.

**Índices:**
```sql
CREATE INDEX IX_Despesas_IdUsuario          ON Despesas (IdUsuario);
CREATE INDEX IX_Despesas_IdCategoria        ON Despesas (IdCategoria);
CREATE INDEX IX_Despesas_IdEstabelecimento  ON Despesas (IdEstabelecimento);
CREATE INDEX IX_Despesas_IdProduto          ON Despesas (IdProduto);
CREATE INDEX IX_Despesas_DataCriacao        ON Despesas (DataCriacao);
```

---

## Diferenças em relação ao CLAUDE.md

O esquema atual do código diverge da documentação em `CLAUDE.md` nos seguintes pontos:

| Ponto                  | CLAUDE.md                  | Código atual                              |
|------------------------|----------------------------|-------------------------------------------|
| Tabela `LinhasProduto` | Não documentada            | Existe — FK em `Marcas`, referenciada em `Produtos` |
| `Produtos.IdMarca`     | NOT NULL                   | NULL (opcional)                           |
| `Despesas.IdProduto`   | NOT NULL                   | NULL (opcional)                           |
| `Despesas.PrecoGranel` | Não documentada            | Existe — REAL nullable                    |
| `Despesas.UnidadeGranel`| Não documentada           | Existe — TEXT nullable                    |
