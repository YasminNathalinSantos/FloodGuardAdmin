# 🌊 FloodGuard Admin — Módulo de Gestão de Ocorrências

API REST em **.NET 10** para gerenciamento administrativo de ocorrências de enchente, equipes de resposta e ações emergenciais — parte da solução **FloodGuard**.

---

## 📋 Índice

- [Sobre o Projeto](#sobre-o-projeto)
- [Arquitetura](#arquitetura)
- [Diagrama de Entidades (ER)](#diagrama-de-entidades-er)
- [Diagrama de Fluxo da API](#diagrama-de-fluxo-da-api)
- [Requisitos](#requisitos)
- [Como Executar](#como-executar)
- [Migrations](#migrations)
- [Endpoints e Testes](#endpoints-e-testes)
- [Tratamento de Erros](#tratamento-de-erros)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)

---

## Sobre o Projeto

O **FloodGuard Admin** é um módulo administrativo que permite:

- Registrar e acompanhar **ocorrências de enchente** com níveis de severidade (Baixo → Crítico)
- Gerenciar **equipes de resposta** e sua disponibilidade
- Criar e controlar **ações emergenciais** (evacuação, socorro, contenção) vinculadas a ocorrências e equipes

---

## Arquitetura

O projeto segue a arquitetura **API REST com Controller → DbContext (EF Core) → Oracle DB**, igual ao padrão da solução de referência.

```
FloodGuardAdmin/
├── Controllers/
│   ├── OcorrenciasController.cs        ← CRUD de ocorrências + regras de negócio
│   ├── EquipesRespostaController.cs     ← CRUD de equipes
│   └── AcoesEmergenciaisController.cs  ← CRUD de ações emergenciais
├── Data/
│   └── AppDbContext.cs                 ← Configuração EF Core + relacionamentos
├── Models/
│   ├── Ocorrencia.cs                   ← Entidade com construtor rico + métodos de negócio
│   ├── EquipeResposta.cs               ← Entidade com construtor rico
│   └── AcaoEmergencial.cs             ← Entidade (lado N dos dois relacionamentos)
├── Migrations/
│   └── 20260601120000_InicializandoDatabase2tdspi.cs
├── Program.cs                          ← Configuração da aplicação + OpenAPI/Scalar
└── appsettings.json                    ← String de conexão Oracle
```

**Por que essa arquitetura?**
Controller acessa o banco diretamente via `AppDbContext` (padrão Repository implícito do EF Core), mantendo o código simples, legível e dentro do escopo da disciplina.

---

## Diagrama de Entidades (ER)

```
┌──────────────────────────┐         ┌──────────────────────────────┐
│  TBL_OCORRENCIAS_2TDSPI  │         │  TBL_EQUIPES_RESPOSTA_2TDSPI │
├──────────────────────────┤         ├──────────────────────────────┤
│ ID              PK       │         │ ID              PK            │
│ TITULO                   │         │ NOME                         │
│ LOCALIZACAO              │         │ ESPECIALIDADE                │
│ NIVEL_SEVERIDADE (1-4)   │         │ CAPACIDADE_MAX               │
│ STATUS                   │         │ DISPONIVEL                   │
│ DATA_OCORRENCIA          │         └──────────────┬───────────────┘
└────────────┬─────────────┘                        │
             │ 1                                    │ 1
             │                                      │
             │ N                                    │ N
             └───────────┬──────────────────────────┘
                         │
          ┌──────────────▼──────────────────────┐
          │  TBL_ACOES_EMERGENCIAIS_2TDSPI       │
          ├─────────────────────────────────────┤
          │ ID              PK                   │
          │ DESCRICAO                            │
          │ TIPO_ACAO                            │
          │ DATA_INICIO                          │
          │ DATA_FIM                             │
          │ STATUS_ACAO                          │
          │ OCORRENCIA_ID       FK (CASCADE)     │
          │ EQUIPE_RESPOSTA_ID  FK (RESTRICT)    │
          └─────────────────────────────────────┘
```

### Relacionamentos implementados

| Relacionamento | Descrição | Delete Behavior |
|---|---|---|
| `Ocorrencia` 1:N `AcaoEmergencial` | Uma ocorrência pode ter várias ações | **CASCADE** — deletar ocorrência remove as ações |
| `EquipeResposta` 1:N `AcaoEmergencial` | Uma equipe pode executar várias ações | **RESTRICT** — não deixa deletar equipe com ações |

---

## Diagrama de Fluxo da API

```
Cliente (Scalar / Postman / Frontend)
        │
        │  HTTP Request
        ▼
┌───────────────────┐
│    Controller     │  ← Valida ModelState, trata erros de negócio
│  (API Layer)      │
└────────┬──────────┘
         │
         │  Consulta / Persistência
         ▼
┌───────────────────┐
│   AppDbContext    │  ← EF Core gerencia SQL, migrations, relacionamentos
│  (Data Layer)     │
└────────┬──────────┘
         │
         │  Oracle SQL
         ▼
┌───────────────────┐
│  Oracle XE 21c    │  ← Banco relacional com FK e CASCADE configurados
│  (Database)       │
└───────────────────┘
```

---

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Oracle Database XE 21c](https://www.oracle.com/database/technologies/xe-downloads.html) (local ou Docker)
- Visual Studio 2022 ou VS Code

---

## Como Executar

### 1. Clone o repositório

```bash
git clone https://github.com/seu-usuario/FloodGuardAdmin.git
cd FloodGuardAdmin
```

### 2. Configure a string de conexão

Edite `FloodGuardAdmin/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=SYSTEM;Password=SUA_SENHA;Data Source=localhost:1521/XEPDB1;"
  }
}
```

### 3. Execute a Migration

```bash
cd FloodGuardAdmin
dotnet ef database update
```

### 4. Execute a aplicação

```bash
dotnet run
```

### 5. Acesse a documentação interativa

Abra no navegador: **https://localhost:7079/swagger**

---

## Migrations

As migrations controlam a evolução do banco de dados ao longo do tempo.

| Comando | Descrição |
|---|---|
| `dotnet ef migrations add NomeDaMigration` | Cria uma nova migration |
| `dotnet ef database update` | Aplica todas as migrations pendentes |
| `dotnet ef database update NomeDaMigration` | Volta para uma migration específica |
| `dotnet ef migrations remove` | Remove a última migration (se não aplicada) |

**Migration criada:** `20260601120000_InicializandoDatabase2tdspi`  
Cria as 3 tabelas com PKs, FKs, índices e Oracle Identity (auto-increment).

---

## Endpoints e Testes

### 🔴 Ocorrências

#### GET /api/ocorrencias
```http
GET http://localhost:5079/api/ocorrencias
```
**Resposta 200:**
```json
[
  {
    "id": 1,
    "titulo": "Alagamento na Av. Paulista",
    "localizacao": "Av. Paulista, 1000 - São Paulo/SP",
    "nivelSeveridade": 3,
    "status": "Aberta",
    "dataOcorrencia": "2025-06-01T08:00:00Z"
  }
]
```

#### POST /api/ocorrencias
```http
POST http://localhost:5079/api/ocorrencias
Content-Type: application/json

{
  "titulo": "Alagamento na Av. Paulista",
  "localizacao": "Av. Paulista, 1000 - São Paulo/SP",
  "nivelSeveridade": 3
}
```
**Resposta 201:** objeto criado com ID gerado automaticamente.

#### PUT /api/ocorrencias/{id}/status
```http
PUT http://localhost:5079/api/ocorrencias/1/status
Content-Type: application/json

"EmAtendimento"
```
**Resposta 204 (NoContent)**

#### PUT /api/ocorrencias/{id}/escalar
```http
PUT http://localhost:5079/api/ocorrencias/1/escalar
```
**Resposta 204 (NoContent)** — eleva o NivelSeveridade em +1

#### DELETE /api/ocorrencias/{id}
```http
DELETE http://localhost:5079/api/ocorrencias/1
```
**Resposta 204 (NoContent)** — remove a ocorrência e todas as ações (CASCADE)

---

### 🟢 Equipes de Resposta

#### POST /api/equipesresposta
```http
POST http://localhost:5079/api/equipesresposta
Content-Type: application/json

{
  "nome": "Equipe Alpha - Resgate",
  "especialidade": "Resgate",
  "capacidadeMax": 12
}
```

#### PUT /api/equipesresposta/{id}/disponibilidade
```http
PUT http://localhost:5079/api/equipesresposta/1/disponibilidade
```
**Resposta 204** — alterna Disponivel entre true/false

#### DELETE /api/equipesresposta/{id}
```http
DELETE http://localhost:5079/api/equipesresposta/1
```
**Resposta 400** se houver ações vinculadas (RESTRICT), **204** se não houver.

---

### 🔵 Ações Emergenciais

#### GET /api/acoesemergenciais/ocorrencia/{ocorrenciaId}
```http
GET http://localhost:5079/api/acoesemergenciais/ocorrencia/1
```

#### POST /api/acoesemergenciais
```http
POST http://localhost:5079/api/acoesemergenciais
Content-Type: application/json

{
  "descricao": "Evacuação de 50 famílias do bairro Vila Esperança",
  "tipoAcao": "Evacuacao",
  "dataInicio": "2025-06-01T08:00:00",
  "statusAcao": "Planejada",
  "ocorrenciaId": 1,
  "equipeRespostaId": 1
}
```

#### PUT /api/acoesemergenciais/{id}/status
```http
PUT http://localhost:5079/api/acoesemergenciais/1/status
Content-Type: application/json

"Concluida"
```
**Resposta 204** — define DataFim automaticamente quando status = "Concluida"

---

## Tratamento de Erros

| Cenário | Código HTTP | Exemplo |
|---|---|---|
| Recurso não encontrado | 404 Not Found | `GET /api/ocorrencias/999` |
| Dados inválidos (ModelState) | 400 Bad Request | POST sem campo obrigatório |
| Regra de negócio violada | 400 Bad Request | Escalar severidade já crítica |
| Status inválido | 400 Bad Request | `"EmAndamento"` (não existe) |
| Exclusão com FK RESTRICT | 400 Bad Request | Deletar equipe com ações |
| Criado com sucesso | 201 Created | POST bem-sucedido |
| Atualizado com sucesso | 204 No Content | PUT / DELETE bem-sucedido |

---

## Tecnologias Utilizadas

| Tecnologia | Versão | Uso |
|---|---|---|
| .NET / ASP.NET Core | 10.0 | Framework da API |
| Entity Framework Core | 10.0.5 | ORM / Migrations |
| Oracle.EntityFrameworkCore | 10.23.26200 | Driver Oracle |
| Oracle Database XE | 21c | Banco de dados relacional |
| Scalar.AspNetCore | 2.14.10 | Documentação interativa |
| Microsoft.AspNetCore.OpenApi | 10.0.5 | Geração do OpenAPI spec |

---

## Integrantes

| Nome | RM |
|---|---|
| Nome do Integrante 1 | RM000000 |
| Nome do Integrante 2 | RM000000 |
| Nome do Integrante 3 | RM000000 |
