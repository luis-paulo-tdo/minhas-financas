# Minhas Finanças

Aplicativo local de gestão de finanças pessoais com autenticação JWT.

## Stack

- **Backend:** .NET 8 Web API + Entity Framework Core + SQLite
- **Frontend:** Angular 21 (standalone components)
- **Banco de dados:** SQLite — arquivo em `backend/MinhasFinancas.Api/banco/minhas-financas.db`

---

## Regras gerais

- Todo o código deve ser escrito em **português**: nomes de classes, propriedades, métodos, variáveis, rotas, tabelas e colunas.
- Autenticação via **JWT Bearer Token** — login com e-mail e senha.
- Sem over-engineering: camadas mínimas, sem abstrações desnecessárias.
- Sem comentários óbvios no código — apenas quando o motivo for não evidente.
- Migrações do EF Core aplicadas automaticamente na inicialização da API.

---

## Banco de Dados

### Diagrama de relacionamentos

```
Usuarios
    │
    └──→ Despesas ──→ Categorias
              ↑
         Estabelecimentos
              ↑
           Produtos ──→ Marcas
```

### Tabela: `Usuarios`

| Coluna       | Tipo    | Restrições                         |
|--------------|---------|------------------------------------|
| `Id`         | INTEGER | PK AUTOINCREMENT                   |
| `Nome`       | TEXT    | NOT NULL                           |
| `Email`      | TEXT    | NOT NULL UNIQUE                    |
| `SenhaHash`  | TEXT    | NOT NULL — bcrypt                  |
| `DataCriacao`| TEXT    | NOT NULL DEFAULT (datetime('now')) |

### Tabela: `Despesas`

| Coluna             | Tipo    | Restrições                          |
|--------------------|---------|-------------------------------------|
| `Id`               | INTEGER | PK AUTOINCREMENT                    |
| `IdUsuario`        | INTEGER | NOT NULL, FK → Usuarios             |
| `IdCategoria`      | INTEGER | NOT NULL, FK → Categorias           |
| `IdEstabelecimento`| INTEGER | NOT NULL, FK → Estabelecimentos     |
| `IdProduto`        | INTEGER | NOT NULL, FK → Produtos             |
| `Descricao`        | TEXT    | NULL                                |
| `Valor`            | REAL    | NOT NULL                            |
| `DataCriacao`      | TEXT    | NOT NULL DEFAULT (datetime('now'))  |

### Tabela: `Categorias`

| Coluna  | Tipo    | Restrições       |
|---------|---------|------------------|
| `Id`    | INTEGER | PK AUTOINCREMENT |
| `Nome`  | TEXT    | NOT NULL UNIQUE  |

Valores fixos: `Essencial`, `Lazer`, `Investimento`

### Tabela: `Estabelecimentos`

| Coluna  | Tipo    | Restrições       |
|---------|---------|------------------|
| `Id`    | INTEGER | PK AUTOINCREMENT |
| `Nome`  | TEXT    | NOT NULL UNIQUE  |

### Tabela: `Produtos`

| Coluna    | Tipo    | Restrições              |
|-----------|---------|-------------------------|
| `Id`      | INTEGER | PK AUTOINCREMENT        |
| `IdMarca` | INTEGER | NOT NULL, FK → Marcas   |
| `Nome`    | TEXT    | NOT NULL                |

### Tabela: `Marcas`

| Coluna  | Tipo    | Restrições       |
|---------|---------|------------------|
| `Id`    | INTEGER | PK AUTOINCREMENT |
| `Nome`  | TEXT    | NOT NULL UNIQUE  |

### Índices

```sql
CREATE INDEX IX_Despesas_IdUsuario          ON Despesas (IdUsuario);
CREATE INDEX IX_Despesas_IdCategoria        ON Despesas (IdCategoria);
CREATE INDEX IX_Despesas_IdEstabelecimento  ON Despesas (IdEstabelecimento);
CREATE INDEX IX_Despesas_IdProduto          ON Despesas (IdProduto);
CREATE INDEX IX_Despesas_DataCriacao        ON Despesas (DataCriacao);
CREATE UNIQUE INDEX IX_Usuarios_Email       ON Usuarios (Email);
```

---

## Backend

### Estrutura de pastas

```
backend/MinhasFinancas.Api/
  Controllers/
    AutenticacaoController.cs
    DespesasController.cs
    CategoriasController.cs
    EstabelecimentosController.cs
    ProdutosController.cs
    MarcasController.cs
  Models/
    Usuario.cs
    Despesa.cs
    Categoria.cs
    Estabelecimento.cs
    Produto.cs
    Marca.cs
  DTOs/
    LoginRequest.cs
    LoginResponse.cs
    RegistroRequest.cs
    DespesaRequest.cs
    DespesaResponse.cs
    FiltrosDespesa.cs
  Data/
    AppDbContext.cs
    Migrations/
  Program.cs
  MinhasFinancas.Api.csproj
```

### Pacotes NuGet

```
Microsoft.EntityFrameworkCore.Sqlite
Microsoft.EntityFrameworkCore.Design
Microsoft.AspNetCore.Authentication.JwtBearer
BCrypt.Net-Next
```

### Endpoints

Base URL: `http://localhost:5000/api`

Todos os endpoints, exceto `/autenticacao/login` e `/autenticacao/registrar`, exigem o header:
`Authorization: Bearer {token}`

O `IdUsuario` é extraído do token JWT — nunca enviado pelo cliente.

#### Autenticação

| Verbo | Rota                          | Descrição                        |
|-------|-------------------------------|----------------------------------|
| POST  | `/autenticacao/registrar`     | Criar conta (nome, e-mail, senha)|
| POST  | `/autenticacao/login`         | Login — retorna JWT              |

**POST `/autenticacao/login`** — request:
```json
{ "email": "usuario@email.com", "senha": "minhasenha" }
```
Response `200 OK`:
```json
{ "token": "eyJ...", "dataExpiracao": "2026-04-19T14:00:00" }
```

#### Despesas

| Verbo  | Rota                        | Descrição                        |
|--------|-----------------------------|----------------------------------|
| GET    | `/despesas`                 | Lista filtrada e paginada        |
| GET    | `/despesas/{id}`            | Detalhe de uma despesa           |
| POST   | `/despesas`                 | Criar despesa                    |
| PUT    | `/despesas/{id}`            | Editar despesa                   |
| DELETE | `/despesas/{id}`            | Excluir despesa                  |
| GET    | `/despesas/resumo`          | Totais por categoria (dashboard) |

Filtros disponíveis no `GET /despesas`: `idCategoria`, `idEstabelecimento`, `idProduto`, `de` (data início), `ate` (data fim), `pagina`, `tamanhoPagina`.

#### Tabelas de apoio (CRUD completo para cada)

| Verbo  | Rota                        |
|--------|-----------------------------|
| GET    | `/categorias`               |
| GET    | `/estabelecimentos`         |
| GET    | `/estabelecimentos/{id}`    |
| POST   | `/estabelecimentos`         |
| PUT    | `/estabelecimentos/{id}`    |
| DELETE | `/estabelecimentos/{id}`    |
| GET    | `/marcas`                   |
| GET    | `/marcas/{id}`              |
| POST   | `/marcas`                   |
| PUT    | `/marcas/{id}`              |
| DELETE | `/marcas/{id}`              |
| GET    | `/produtos`                 |
| GET    | `/produtos/{id}`            |
| POST   | `/produtos`                 |
| PUT    | `/produtos/{id}`            |
| DELETE | `/produtos/{id}`            |

### Validações (DespesaRequest)

- `IdCategoria`, `IdEstabelecimento`, `IdProduto`: obrigatórios, devem existir no banco
- `Valor`: obrigatório, maior que zero
- `DataCriacao`: opcional — usa `datetime('now')` se não informado

---

## Frontend

### Estrutura de pastas

```
frontend/minhas-financas-ui/src/app/
  core/
    models/
      despesa.model.ts
      categoria.model.ts
      estabelecimento.model.ts
      produto.model.ts
      marca.model.ts
    services/
      autenticacao.service.ts
      despesa.service.ts
      categoria.service.ts
      estabelecimento.service.ts
      produto.service.ts
      marca.service.ts
    interceptors/
      autenticacao.interceptor.ts
    guards/
      autenticacao.guard.ts
  features/
    autenticacao/
      login/
        login.component.ts
        login.component.html
        login.component.scss
      registro/
        registro.component.ts
        registro.component.html
        registro.component.scss
    despesas/
      lista-despesas/
        lista-despesas.component.ts
        lista-despesas.component.html
        lista-despesas.component.scss
      formulario-despesa/
        formulario-despesa.component.ts
        formulario-despesa.component.html
        formulario-despesa.component.scss
      filtro-despesas/
        filtro-despesas.component.ts
        filtro-despesas.component.html
    painel/
      painel.component.ts
      painel.component.html
  shared/
    components/
      dialogo-confirmacao/
        dialogo-confirmacao.component.ts
    pipes/
      etiqueta-categoria.pipe.ts
      moeda-brl.pipe.ts
  app.component.ts
  app.routes.ts
```

### Rotas Angular

| Rota                     | Componente                   | Guard              |
|--------------------------|------------------------------|--------------------|
| `/`                      | redireciona para `/despesas` | —                  |
| `/login`                 | `LoginComponent`             | —                  |
| `/registro`              | `RegistroComponent`          | —                  |
| `/despesas`              | `ListaDespesasComponent`     | `AutenticacaoGuard`|
| `/despesas/nova`         | `FormularioDespesaComponent` | `AutenticacaoGuard`|
| `/despesas/:id/editar`   | `FormularioDespesaComponent` | `AutenticacaoGuard`|
| `/painel`                | `PainelComponent`            | `AutenticacaoGuard`|

`AutenticacaoGuard` redireciona para `/login` se não houver token válido no `localStorage`.

### Métodos do `AutenticacaoService`

```typescript
login(req: LoginRequest): Observable<LoginResponse>   // salva token no localStorage
registrar(req: RegistroRequest): Observable<void>
logout(): void                                         // remove token do localStorage
obterUsuarioAtual(): UsuarioLogado | null
estaAutenticado(): boolean
```

O `AutenticacaoInterceptor` injeta automaticamente o header `Authorization: Bearer {token}` em todas as requisições ao `apiUrl`.

### Métodos do `DespesaService`

```typescript
obterDespesas(filtros: FiltrosDespesa): Observable<ResultadoPaginado<DespesaResponse>>
obterDespesa(id: number): Observable<DespesaResponse>
criarDespesa(req: DespesaRequest): Observable<DespesaResponse>
atualizarDespesa(id: number, req: DespesaRequest): Observable<DespesaResponse>
excluirDespesa(id: number): Observable<void>
obterResumo(de?: string, ate?: string): Observable<ResumoDespesas>
```

### Telas

**`/despesas`** — tabela com colunas: Data, Estabelecimento, Produto, Marca, Categoria (badge colorido), Valor, Ações (editar / excluir). Barra de filtros colapsável no topo. Paginação na base.

**`/despesas/nova` e `/despesas/:id/editar`** — formulário reativo com os campos: Data/hora, Estabelecimento (select + criar novo), Produto (select + criar novo), Descrição, Valor, Categoria.

**`/login`** — formulário com e-mail e senha. Link para `/registro`. Ao autenticar, redireciona para `/despesas`.

**`/registro`** — formulário com nome, e-mail e senha. Ao registrar, redireciona para `/login`.

**`/painel`** — 3 cards (Essencial / Lazer / Investimento) com total do período. Seletor de intervalo de datas no topo. Gráfico de barras por categoria.

---

## Como rodar

### Pré-requisitos

```bash
dotnet --version       # 8.x
node --version         # 20+
ng version             # 21.x  →  npm install -g @angular/cli@21
dotnet tool install --global dotnet-ef
```

### Backend

```bash
cd backend/MinhasFinancas.Api
dotnet restore
dotnet ef migrations add CriacaoInicial   # apenas na primeira vez
dotnet run
# API disponível em http://localhost:5000
```

### Frontend

```bash
cd frontend/minhas-financas-ui
npm install
ng serve
# App disponível em http://localhost:4200
```

### Build combinado (opcional)

```bash
# Compila o Angular dentro do wwwroot da API
ng build --output-path ../../backend/MinhasFinancas.Api/wwwroot

# Em Program.cs: app.UseStaticFiles(); app.MapFallbackToFile("index.html");
# Tudo servido em http://localhost:5000
```

---

## Sequência de implementação

### Backend
- [x] Criar solução .NET e projeto Web API
- [x] Definir entidades e `AppDbContext`, gerar migration inicial
- [x] Alimentar banco com dados mockados (`Data/Seed.cs`)
- [ ] Implementar `AutenticacaoController` (registrar + login com JWT)
- [ ] Implementar controllers das tabelas de apoio (Marcas, Produtos, Estabelecimentos, Categorias)
- [ ] Implementar `DespesasController` com todos os endpoints incluindo resumo
- [ ] Testar a API com arquivo `.http`

### Frontend
- [x] Criar projeto Angular com `ng new`
- [x] Criar models e services base (`LoginRequest`, `LoginResponse`, `Usuario`)
- [x] Implementar `AutenticacaoService`, `AutenticacaoInterceptor` e `AutenticacaoGuard`
- [x] Construir tela de login e shell principal com menu
- [ ] Construir `ListaDespesasComponent` + `FiltroDespesasComponent`
- [ ] Construir `FormularioDespesaComponent`
- [ ] Construir `PainelComponent`
- [ ] Testar fluxo completo end-to-end
