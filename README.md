# 🌊 Aquasafe API FloodGuard Admin — Módulo de Gestão de Ocorrências

API REST em **.NET 9.0** para gerenciamento administrativo de ocorrências de enchente, equipes de resposta e ações emergenciais — parte da solução **FloodGuard**.

---

## 📋 Índice

- [Sobre o Projeto](#sobre-o-projeto)
- [Regras de Negócio](#regras-de-negócio)
- [Arquitetura](#arquitetura)
- [Diagrama de Entidades (ER)](#diagrama-de-entidades-er)
- [Diagrama de Fluxo da API](#diagrama-de-fluxo-da-api)
- [Requisitos](#requisitos)
- [Como Executar](#como-executar)
- [Migrations](#migrations)
- [Endpoints e Testes](#endpoints-e-testes)
- [Tratamento de Erros](#tratamento-de-erros)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Equipe](#equipe)
- [Links](#links)

---
## 👥 Equipe

| Nome | RM |
|------|----|
| Yasmin Nathalin | RM561365 |
| Lucas da Silva Lima | RM562118 |
| Riquelme Nascimento de Oliveira | RM565468 |
| Enzo Franchin de Souza | RM565677 |

---

## 🔗 Links

| Recurso | URL |
|---------|-----|
| 🎥 **Vídeo de Demonstração** | Em breve |
| 🎯 **Vídeo Pitch** | Em breve |
| 📘 **Swagger UI** | https://localhost:7079/swagger |

## Sobre o Projeto

O **FloodGuard Admin** é um módulo administrativo que permite:

- Registrar e acompanhar **ocorrências de enchente** com níveis de severidade (Baixo → Crítico)
- Gerenciar **equipes de resposta** e sua disponibilidade
- Criar e controlar **ações emergenciais** (evacuação, socorro, contenção) vinculadas a ocorrências e equipes

---

## Regras de Negócio — Níveis de Risco

| Nível de Severidade | Risco Calculado |
|---------------------|-----------------|
| 1 | 🟢 BAIXO |
| 2 | 🟡 MÉDIO |
| 3 | 🟠 ALTO |
| 4 | 🔴 CRÍTICO |

---

## Arquitetura

O projeto segue a arquitetura **API REST com Controller → DbContext (EF Core) → Oracle DB**.

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
    ├── Program.cs                          ← Configuração da aplicação + Swagger
    └── appsettings.json                    ← String de conexão Oracle

**Por que essa arquitetura?**
Controller acessa o banco diretamente via `AppDbContext` (padrão Repository implícito do EF Core), mantendo o código simples, legível e dentro do escopo da disciplina.
---

## Diagrama de Entidades (ER)

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

### Relacionamentos implementados

| Relacionamento | Descrição | Delete Behavior |
|---|---|---|
| Ocorrencia 1:N AcaoEmergencial | Uma ocorrência pode ter várias ações | **CASCADE** — deletar ocorrência remove as ações |
| EquipeResposta 1:N AcaoEmergencial | Uma equipe pode executar várias ações | **RESTRICT** — não deixa deletar equipe com ações |

---

## Diagrama de Fluxo da API

    Cliente (Swagger / Postman / Frontend)
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
    │   Oracle FIAP     │  ← Banco relacional com FK e CASCADE configurados
    │  (Database)       │
    └───────────────────┘
    ---

## Requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Visual Studio 2022 ou VS Code
- Acesso ao banco Oracle FIAP

---

## Como Executar

### 1. Clone o repositório

    git clone https://github.com/seu-usuario/FloodGuardAdmin.git
    cd FloodGuardAdmin

### 2. Configure a string de conexão

Crie o arquivo FloodGuardAdmin/appsettings.json com base no appsettings.Example.json:

    {
      "ConnectionStrings": {
        "OracleConnection": "User Id=SEU_RM;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/ORCL;"
      }
    }

### 3. Execute a aplicação

    dotnet run

As tabelas são criadas automaticamente no banco ao iniciar a aplicação.

### 4. Acesse a documentação interativa

Abra no navegador: https://localhost:7079/swagger

---

## Migrations

As migrations controlam a evolução do banco de dados ao longo do tempo.

| Comando | Descrição |
|---|---|
| dotnet ef migrations add NomeDaMigration | Cria uma nova migration |
| dotnet ef database update | Aplica todas as migrations pendentes |
| dotnet ef database update NomeDaMigration | Volta para uma migration específica |
| dotnet ef migrations remove | Remove a última migration (se não aplicada) |

**Migration criada:** 20260601120000_InicializandoDatabase2tdspi
Cria as 3 tabelas com PKs, FKs, índices e Oracle Identity (auto-increment).
---

## Endpoints e Testes

### 🔴 Ocorrências

#### GET /api/ocorrencias
    GET https://localhost:7079/api/ocorrencias

#### POST /api/ocorrencias
    POST https://localhost:7079/api/ocorrencias
    Content-Type: application/json

    {
      "titulo": "Alagamento na Av. Paulista",
      "localizacao": "Av. Paulista, 1000 - São Paulo/SP",
      "nivelSeveridade": 3
    }

Resposta 201: objeto criado com ID gerado automaticamente.

#### PUT /api/ocorrencias/{id}/status
    PUT https://localhost:7079/api/ocorrencias/1/status
    Content-Type: application/json

    "EmAtendimento"

#### PUT /api/ocorrencias/{id}/escalar
    PUT https://localhost:7079/api/ocorrencias/1/escalar

Eleva o NivelSeveridade em +1.

#### DELETE /api/ocorrencias/{id}
    DELETE https://localhost:7079/api/ocorrencias/1

Remove a ocorrência e todas as ações vinculadas (CASCADE).

---

### 🟢 Equipes de Resposta

#### GET /api/equipesresposta
    GET https://localhost:7079/api/equipesresposta

#### POST /api/equipesresposta
    POST https://localhost:7079/api/equipesresposta
    Content-Type: application/json

    {
      "nome": "Equipe Alpha - Resgate",
      "especialidade": "Resgate",
      "capacidadeMax": 12
    }

#### PUT /api/equipesresposta/{id}/disponibilidade
    PUT https://localhost:7079/api/equipesresposta/1/disponibilidade

Alterna disponível/indisponível.

#### DELETE /api/equipesresposta/{id}
    DELETE https://localhost:7079/api/equipesresposta/1

Retorna 400 se houver ações vinculadas (RESTRICT).

---

### 🔵 Ações Emergenciais

#### GET /api/acoesemergenciais/ocorrencia/{ocorrenciaId}
    GET https://localhost:7079/api/acoesemergenciais/ocorrencia/1

#### POST /api/acoesemergenciais
    POST https://localhost:7079/api/acoesemergenciais
    Content-Type: application/json

    {
      "descricao": "Evacuação de 50 famílias do bairro Vila Esperança",
      "tipoAcao": "Evacuacao",
      "dataInicio": "2025-06-01T08:00:00",
      "statusAcao": "Planejada",
      "ocorrenciaId": 1,
      "equipeRespostaId": 1
    }

#### PUT /api/acoesemergenciais/{id}/status
    PUT https://localhost:7079/api/acoesemergenciais/1/status
    Content-Type: application/json

    "Concluida"

Define DataFim automaticamente quando status = "Concluida".
---

## Tratamento de Erros

| Cenário | Código HTTP | Resposta |
|---|---|---|
| Recurso não encontrado | 404 Not Found | { "erro": "Ocorrência com Id 99 não encontrada." } |
| Dados inválidos | 400 Bad Request | { "erro": "Dados inválidos.", "detalhes": [...] } |
| Regra de negócio violada | 400 Bad Request | { "erro": "A ocorrência já está no nível crítico." } |
| Status inválido | 400 Bad Request | { "erro": "...", "statusValidos": [...] } |
| Exclusão com FK RESTRICT | 400 Bad Request | { "erro": "...", "solucao": "..." } |
| Criado com sucesso | 201 Created | Objeto criado com ID gerado |
| Operação bem-sucedida | 200 OK | { "mensagem": "..." } |

---

## Tecnologias Utilizadas

| Tecnologia | Versão | Uso |
|---|---|---|
| .NET / ASP.NET Core | 9.0 | Framework da API |
| Entity Framework Core | 9.0.5 | ORM / Migrations |
| Oracle.EntityFrameworkCore | 9.23.60 | Driver Oracle |
| Oracle Database FIAP | - | Banco de dados relacional |
| Swashbuckle.AspNetCore | 6.9.0 | Swagger UI |

---
