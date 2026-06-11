# Painel de Obrigações Acessórias · e-Auditoria

Aplicação SaaS de gestão tributária para escritórios contábeis controlarem
obrigações acessórias (DAS, DCTF, EFD, eSocial, SPED, etc.) de múltiplos CNPJs.

O coração do sistema é a **`TaxObligationEngine`**: uma engine pura que, dado um
regime tributário e uma competência (ano/mês), calcula quais obrigações se aplicam
e seus respectivos vencimentos — sem persistir nada. Apenas as **entregas** são
gravadas no banco; o status de cada obrigação é resolvido em runtime cruzando o
resultado da engine com as entregas registradas.

---

## Stack

| Camada    | Tecnologia |
|-----------|------------|
| Frontend  | React 18 + Vite + TypeScript + Ant Design 5 + TanStack Query |
| Backend   | .NET 9 — Minimal APIs + Clean Architecture |
| Banco     | PostgreSQL 16 + EF Core |
| Infra     | Docker + Docker Compose (um comando sobe tudo) |
| Testes    | xUnit — unitários (engine) + integração (WebApplicationFactory + Testcontainers/PostgreSQL) |

---

## Como subir

Pré-requisito: Docker + Docker Compose.

```bash
# 1. Suba os três serviços
docker compose up --build
```

Serviços expostos:

| Serviço   | URL / Porta                       |
|-----------|-----------------------------------|
| Frontend  | http://localhost:5173             |
| API       | http://localhost:5000             |
| Swagger   | http://localhost:5000/swagger     |
| Health    | http://localhost:5000/health      |
| Postgres  | localhost:5432                    |

Verificação rápida:

```bash
curl http://localhost:5000/health      # → {"status":"healthy"}
```

---

## Estrutura do monorepo

```
painel_obrigacoes_acessorias/
├── backend/
│   ├── EAuditoria.slnx
│   ├── Dockerfile                 # multi-stage: sdk:9.0 → aspnet:9.0
│   ├── src/
│   │   ├── Domain/                # Entidades (Empresa, Entrega), enums, TaxObligationEngine — ZERO deps externas
│   │   ├── Application/           # DTOs, services (use cases), interfaces de repositório, validação de CNPJ (→ Domain)
│   │   ├── Infrastructure/        # EF Core + Npgsql: AppDbContext, configs, migrations, seed, repos (→ Application)
│   │   └── API/                   # Minimal API endpoints + middleware de exceções (→ Infrastructure)
│   └── tests/
│       ├── EAuditoria.Tests/             # xUnit unitário (→ Domain)
│       └── EAuditoria.IntegrationTests/  # WebApplicationFactory + Testcontainers (→ API)
├── frontend/                      # React + Vite + Ant Design
│   ├── Dockerfile                 # build Node → nginx
│   └── nginx.conf                 # SPA fallback
├── docker-compose.yml             # postgres · api · frontend
├── .env.example
└── CLAUDE.md                      # guia de arquitetura e regras de negócio
```

---

## Arquitetura

### Backend — Clean Architecture em 4 camadas

```
Domain  ←  Application  ←  Infrastructure  ←  API
```

- **Regra de ouro:** `Domain` não referencia nenhum pacote NuGet externo. A
  `TaxObligationEngine` é pura (sem I/O) e 100% testável com xUnit.
- **Minimal APIs** em vez de MVC: organização limpa sem o overhead do controller.
- **Sem CQRS/MediatR:** o escopo não justifica o boilerplate.

### Frontend — Feature-based

Cada domínio (`companies`, `calendar`, `alerts`, `dashboard`) é autocontido
(`components/`, `hooks/`, `types.ts`). Estado de servidor via **TanStack Query**
(cache + invalidation após mutations); estado de UI é local. Sem Redux/Zustand.

```
frontend/src/
├── features/
│   ├── dashboard/        # KPIs por competência (useDashboard)
│   ├── companies/        # CRUD de empresas (useCompanies/useCreateCompany/useDeleteCompany)
│   ├── calendar/         # Calendário de obrigações + exportação CSV/PDF (useObligations)
│   └── alerts/           # Painel de alertas: atrasadas + a vencer (useAlerts)
├── shared/
│   ├── components/       # AppLayout, RegimeBadge, StatusBadge, MarkDeliveryButton, CompanySelect
│   ├── hooks/            # useDeliveries (marcar/desfazer), useDebouncedValue
│   ├── services/         # apiClient (axios)
│   ├── theme/            # paleta de cores
│   ├── types/            # enums (espelham o backend), PagedResult, DTOs
│   └── utils/            # máscara/validação de CNPJ, formatação de data, exportação CSV/PDF
└── App.tsx               # BrowserRouter + rotas
```

O shell usa Ant Design `Layout` (Sider colapsável com menu Dashboard/Empresas/
Calendário/Alertas e Header com título por rota). O cliente axios aponta para
`VITE_API_URL` (default `http://localhost:5000/api`).

As páginas são carregadas sob demanda (`React.lazy` + `Suspense`) e as libs
grandes ficam em chunks de vendor próprios (`antd`, `react`) para melhor cache.
Um interceptor global do axios reporta falhas inesperadas (rede ou 5xx) via
`notification`; erros de negócio 4xx são tratados nas próprias telas.

O `CompanySelect` (usado no Calendário e no filtro de Alertas) faz **busca
server-side** com debounce — consulta `/companies?search=` e mantém o rótulo da
empresa selecionada — escalando para muitas empresas sem carregar todas de uma vez.
O Painel de Alertas pagina cada grupo (atrasadas / a vencer) no cliente.

O Calendário permite **exportar a competência em CSV ou PDF** (empresa +
competência selecionadas). O CSV é gerado sem dependências (separador `;` + BOM
para o Excel pt-BR); o PDF usa jsPDF carregado por **import dinâmico** — só baixa
ao clicar em exportar, sem impactar o carregamento inicial.

---

## Persistência (modelo de dados)

Apenas duas tabelas são persistidas — as obrigações **não** são gravadas, e sim
calculadas em runtime pela engine.

**`Empresas`**

| Coluna | Tipo | Observação |
|--------|------|------------|
| Id | uuid | PK |
| NomeFantasia | varchar(200) | obrigatório |
| Cnpj | char(14) | apenas dígitos · **único** |
| Regime | int | enum `RegimeTributario` |
| CreatedAt | timestamp | UTC |

**`Entregas`**

| Coluna | Tipo | Observação |
|--------|------|------------|
| Id | uuid | PK |
| EmpresaId | uuid | FK → Empresas (cascade) |
| TipoObrigacao | int | enum `TipoObrigacao` |
| CompetenciaAno / CompetenciaMes | int | |
| DataEntrega | date | |
| CreatedAt | timestamp | UTC |

- Índice único `IX_Empresas_Cnpj`.
- Índice único `IX_Entregas (EmpresaId, TipoObrigacao, CompetenciaAno, CompetenciaMes)`
  — impede entrega duplicada para a mesma competência.

**Migrations & seed.** Na inicialização, um `IHostedService` (`DatabaseSeeder`)
aplica as migrations pendentes e, se o banco estiver vazio, insere **10 empresas de
demonstração** (3 Simples Nacional, 3 Lucro Presumido, 2 Lucro Real, 2 Imunidade/Isenção). O `CNPJ` é
armazenado apenas com dígitos; a máscara de exibição fica no frontend.

---

## API (endpoints)

Documentação interativa em **http://localhost:5000/swagger**. Erros seguem
ProblemDetails (RFC 7807): validação → **400** (com campo `errors`), não
encontrado → **404**, conflito → **409**.

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/health` | Health check → `{ "status": "healthy" }` |
| GET | `/api/companies?page=&pageSize=&search=` | Lista empresas paginadas (`PagedResult`); `search` filtra por nome (ILIKE) ou CNPJ |
| POST | `/api/companies` | Cadastra empresa (valida dígito verificador do CNPJ; 409 se duplicado) |
| DELETE | `/api/companies/{id}` | Remove empresa (cascade nas entregas) |
| GET | `/api/obligations?empresaId=&year=&month=` | Obrigações calculadas pela engine com status real (Pendente/Atrasada/Entregue) |
| POST | `/api/deliveries` | Registra entrega (409 se já existe para a competência) |
| DELETE | `/api/deliveries/{id}` | Remove a entrega (desfaz a marcação) |
| GET | `/api/alerts?diasAdiante=30` | Atrasadas + a vencer em N dias (todas as empresas), ordenadas por urgência |
| GET | `/api/dashboard?year=&month=` | KPIs: total de empresas, obrigações do mês, pendentes, entregues, atrasadas |

O status de cada obrigação é resolvido cruzando o resultado da `TaxObligationEngine`
com a tabela `Entregas`: existe entrega → **Entregue**; senão, vencimento no passado
→ **Atrasada**; caso contrário → **Pendente**.

---

## Regras de negócio

### Obrigações por regime

| Obrigação         | Periodicidade | Simples | L.Presumido | L.Real |
|-------------------|---------------|:-------:|:-----------:|:------:|
| DAS               | Mensal        | ✓ | — | — |
| DEFIS             | Anual (jan)   | ✓ | — | — |
| DCTF              | Mensal        | — | ✓ | ✓ |
| EFD-ICMS/IPI      | Mensal        | — | ✓ | ✓ |
| EFD Contribuições | Mensal        | — | ✓ | ✓ |
| EFD-Reinf         | Mensal        | — | ✓ | ✓ |
| SPED ECD          | Anual (jan)   | — | ✓ | ✓ |
| SPED ECF          | Anual (jan)   | — | ✓ | ✓ |
| eSocial           | Mensal        | ✓ | ✓ | ✓ |
| DIRF              | Anual (jan)   | ✓ | ✓ | ✓ |
| RAIS              | Anual (jan)   | ✓ | ✓ | ✓ |
| **Imunidade/Isenção** | —         | nenhuma obrigação | | |

Obrigações **anuais aparecem apenas na competência de janeiro**.

### Vencimentos

| Obrigação         | Prazo |
|-------------------|-------|
| DAS               | Dia 20 do mês seguinte |
| DCTF              | Dia 15 do **segundo** mês seguinte |
| EFD-ICMS/IPI      | Dia 15 do mês seguinte |
| EFD Contribuições | Dia 15 do mês seguinte |
| EFD-Reinf         | Dia 15 do mês seguinte |
| eSocial           | Dia 7 do mês seguinte |
| SPED ECD          | 31 de maio do ano seguinte |
| SPED ECF          | 31 de julho do ano seguinte |
| DIRF              | Último dia de fevereiro do ano seguinte |
| RAIS / DEFIS      | 31 de março do ano seguinte |

### Status de uma obrigação

- **Pendente** — vencimento futuro, sem entrega registrada
- **Atrasada** — vencimento passado, sem entrega registrada
- **Entregue** — existe registro na tabela `Entregas`
- **Não Aplicável** — obrigação não se aplica ao regime da empresa

---

## Decisões técnicas

### Por que obrigações não ficam no banco

| Abordagem | Prós | Contras |
|-----------|------|---------|
| Persistir obrigações | Consulta simples | Duplicidade enorme (N CNPJs × 12 meses × M obrigações); inconsistência quando as regras mudam |
| **Calcular em runtime** ✅ | Sempre consistente; regras mudam sem migração; banco enxuto | Engine precisa ser rápida (é pura, sem I/O → muito rápida) |

A engine é determinística: recebe `(regime, ano, mês)` e devolve as obrigações.
O backend cruza esse resultado com as `Entregas` para definir o status final.

### Decisões sobre ambiguidades do case

| Ambiguidade | Decisão |
|-------------|---------|
| Imunidade/Isenção — quais obrigações? | Nenhuma (regime isento por definição) |
| eSocial para Imunidade? | Não incluído (conservador) |
| Semântica do `year` nas obrigações anuais | O `year` é o **exercício**; o vencimento cai no ano seguinte. Ex.: `Calculate(LucroReal, 2024, 1)` → SPED ECD vence 31/05/2025. Anuais só são emitidas quando `month == 1`. |
| Prorrogação de fim de semana | Aplicada **apenas às obrigações mensais** (sáb/dom → próximo dia útil). As datas legais fixas das anuais **não** são prorrogadas (ex.: SPED ECD em 31/05/2025, um sábado, permanece). Feriados não são considerados. |
| CNPJ — validar dígito verificador? | Sim — validado no backend (`POST /api/companies` → 400); a máscara/validação client-side no formulário fica para o frontend |
| Feriados nacionais no cálculo de dias úteis? | Não — apenas fim de semana, conforme o case |

---

## Testes

O backend tem como alvo `net9.0`. Com o SDK .NET 9 disponível:

```bash
cd backend
dotnet test                 # toda a solução (unitários + integração)
```

### Unitários — `EAuditoria.Tests` (→ Domain)
Cobrem a `TaxObligationEngine`: conjunto de obrigações por regime (mês comum e
janeiro), Imunidade vazia, vencimentos mensais e anuais, prorrogação do DAS em
sábado e domingo, DCTF com delay de 2 meses, fevereiro bissexto na DIRF e validação
de mês inválido. São puros (sem I/O) e rápidos.

```bash
dotnet test tests/EAuditoria.Tests          # só os unitários
```

### Integração — `EAuditoria.IntegrationTests` (→ API)
Sobem a API em memória (`WebApplicationFactory`) contra um **PostgreSQL 16 real
provisionado via Testcontainers**, com migrations e seed automáticos. Exercitam os
endpoints por HTTP de ponta a ponta: health, CRUD de empresas (201 / 400 com
`errors` / 409 / 404), cálculo de obrigações com status, fluxo de entrega
(marcar → 409 em duplicata → desfazer) e os agregados de dashboard/alertas. Usam
CNPJs aleatórios válidos para não dependerem de um banco limpo.

```bash
dotnet test tests/EAuditoria.IntegrationTests   # só os de integração (requer Docker)
```

> **Requisito:** os testes de integração precisam do Docker em execução — o
> Testcontainers cria e derruba o container do Postgres automaticamente.

> Em ambientes sem .NET 9 instalado, os testes também rodam dentro da imagem oficial:
> ```bash
> docker run --rm -v "$PWD/backend:/src" -w /src mcr.microsoft.com/dotnet/sdk:9.0 dotnet test
> ```

---

## Status de implementação

| Bloco | Escopo | Status |
|-------|--------|:------:|
| 1 | Setup do monorepo (Docker, scaffold backend/frontend, /health) | ✅ |
| 2 | Domain: enums, `TaxObligationEngine` + testes xUnit | ✅ |
| 3 | Infrastructure: entidades EF Core, `AppDbContext`, migration `InitialCreate`, seed automático | ✅ |
| 4 | API: endpoints companies / obligations / deliveries / alerts / dashboard (verificados end-to-end) | ✅ |
| 5 | Frontend: shell (layout, rotas, apiClient) + Dashboard com KPIs | ✅ |
| 6 | Frontend: Gestão de Empresas (tabela, cadastro com máscara/validação de CNPJ, exclusão) | ✅ |
| 7 | Frontend: Calendário de Obrigações (filtros, status, marcar/desfazer entrega) | ✅ |
| 8 | Frontend: Painel de Alertas (atrasadas + a vencer, marcar entrega inline) | ✅ |
| 9 | Polish: favicon, erro global (axios → notification), code-splitting por rota, responsividade | ✅ |

---

## Uso de IA

Desenvolvido com apoio do **Claude Code**: scaffold inicial do monorepo,
implementação da `TaxObligationEngine` e dos testes unitários, a camada de
persistência (entidades, `AppDbContext`, migration e seed automático), a API
completa (DTOs, services, repositórios e endpoints) e o frontend completo
(shell de layout, Dashboard, Empresas, Calendário e Painel de Alertas com seus
hooks TanStack Query) — guiados por
*skills* específicas do projeto (`tax-engine`, `new-endpoint`, `new-feature`,
`check-and-test`, `git-commit`) versionadas em `.claude/skills/`. Decisões de
regra de negócio (semântica das anuais, prorrogação só nas mensais) foram validadas
manualmente contra a tabela de vencimentos do case.
