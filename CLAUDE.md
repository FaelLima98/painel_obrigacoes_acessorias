# Painel de Obrigações Acessórias — e-Auditoria

## O que é este projeto
Aplicação SaaS de gestão tributária para escritórios contábeis controlarem obrigações
acessórias (DCTF, EFD, DAS, eSocial, etc.) de múltiplos CNPJs. Stack obrigatória:
React 18 + Vite + TypeScript + Ant Design no frontend, .NET 9 Minimal APIs + Clean
Architecture no backend, PostgreSQL 16 + EF Core, Docker Compose.

---

## Arquitetura do Backend

```
backend/src/
├── Domain/           # Entidades, enums, TaxObligationEngine — ZERO dependências externas
├── Application/      # Use cases e DTOs — depende só do Domain
├── Infrastructure/   # EF Core, Repos — depende de Application + Domain
└── API/              # Minimal API endpoints — depende de tudo acima
```

**Regra de ouro:** `Domain` não referencia nenhum pacote NuGet externo.
A `TaxObligationEngine` é pura (sem I/O) e deve ser totalmente testável com xUnit.

### Padrão de endpoint Minimal API
```csharp
// Em API/Endpoints/XxxEndpoints.cs
public static class XxxEndpoints
{
    public static IEndpointRouteBuilder MapXxxEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/xxx").WithTags("Xxx");
        group.MapGet("/", Handler);
        return app;
    }

    private static async Task<IResult> Handler(...) { ... }
}
// Em Program.cs: app.MapXxxEndpoints();
```

### Tratamento de erros
- Retornar sempre `Results.Problem(...)` (ProblemDetails RFC 7807)
- Erros de validação: HTTP 400 com campo `errors`
- Conflito (ex: CNPJ duplicado): HTTP 409
- Não encontrado: HTTP 404

---

## Arquitetura do Frontend

```
frontend/src/
├── features/         # Um diretório por domínio (companies, calendar, alerts, dashboard)
│   └── companies/
│       ├── components/
│       ├── hooks/        # useCompanies(), useCreateCompany(), etc.
│       └── types.ts
├── shared/
│   ├── components/   # StatusBadge, RegimeBadge, etc.
│   ├── services/     # apiClient (axios) com interceptors
│   └── types/        # tipos globais
└── App.tsx           # Rotas + Ant Design Layout
```

### Padrão de hook TanStack Query
```typescript
// Sempre invalidar queries relacionadas após mutations
export function useCreateCompany() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateCompanyDto) => api.post('/companies', data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['companies'] }),
  });
}
```

### Convenções React
- Sem `any` no TypeScript — sempre tipar
- Componentes funcionais + hooks
- Sem Redux/Zustand — estado servidor via TanStack Query, UI state local

---

## Paleta de Cores (Ant Design tokens)
```css
--primary:    #1565C0   /* botões, links, cabeçalhos */
--primary-h:  #1E88E5   /* hover, badges informativos */
--accent:     #00ACC1   /* progresso, tags positivas */
--dark:       #0D1B2A   /* header, sidebar */
--success:    #2E7D32   /* status Entregue */
--warning:    #F57F17   /* prazos próximos (< 7 dias) */
--danger:     #C62828   /* atrasadas, erros */
--surface:    #F5F7FA   /* fundo de cards */
```

---

## Regras de Negócio Críticas

### Obrigações por Regime
| Obrigação         | Periodicidade | Simples | L.Presumido | L.Real |
|-------------------|--------------|---------|-------------|--------|
| DAS               | Mensal        | ✓       | —           | —      |
| DEFIS             | Anual (jan)   | ✓       | —           | —      |
| DCTF              | Mensal        | —       | ✓           | ✓      |
| EFD-ICMS/IPI      | Mensal        | —       | ✓           | ✓      |
| EFD Contribuições | Mensal        | —       | ✓           | ✓      |
| EFD-Reinf         | Mensal        | —       | ✓           | ✓      |
| SPED ECD          | Anual (jan)   | —       | ✓           | ✓      |
| SPED ECF          | Anual (jan)   | —       | ✓           | ✓      |
| eSocial           | Mensal        | ✓       | ✓           | ✓      |
| DIRF              | Anual (jan)   | ✓       | ✓           | ✓      |
| RAIS              | Anual (jan)   | ✓       | ✓           | ✓      |
| **Imunidade**     | —             | nenhuma |             |        |

**Obrigações anuais aparecem APENAS em janeiro.**

### Vencimentos
| Obrigação         | Prazo                                              |
|-------------------|----------------------------------------------------|
| DAS               | Dia 20 do mês seguinte (proroga se fim de semana)  |
| DCTF              | Dia 15 do **segundo** mês seguinte                 |
| EFD-ICMS/IPI      | Dia 15 do mês seguinte                             |
| EFD Contribuições | Dia 15 do mês seguinte                             |
| eSocial           | Dia 7 do mês seguinte                              |
| EFD-Reinf         | Dia 15 do mês seguinte                             |
| SPED ECD          | 31 de maio do ano seguinte                         |
| SPED ECF          | 31 de julho do ano seguinte                        |
| DIRF              | Último dia de fevereiro do ano seguinte            |
| RAIS / DEFIS      | 31 de março do ano seguinte                        |

**Prorogação:** só sábado/domingo → próximo dia útil (feriados não considerados).

### Status
- **Pendente** — vencimento futuro, sem entrega registrada
- **Atrasada** — vencimento passado, sem entrega registrada
- **Entregue** — existe registro na tabela `Entregas`
- **Não Aplicável** — obrigação não se aplica ao regime da empresa

---

## Decisões de Design

1. **Obrigações não são persistidas** — calculadas em runtime pela `TaxObligationEngine`.
   Apenas `Entregas` ficam no banco. Evita inconsistência quando regras mudam.

2. **Índice único em Entregas** — `(EmpresaId, TipoObrigacao, CompetenciaAno, CompetenciaMes)`
   previne duplicidade de entrega para a mesma competência.

3. **Seed automático** — roda via `IHostedService` na inicialização se banco vazio.
   8 empresas demo: 3 Simples, 3 L.Presumido, 2 L.Real.

---

## Comandos Úteis

```bash
# Subir tudo
docker compose up --build

# Rodar testes do backend
cd backend && dotnet test

# Frontend local (sem Docker)
cd frontend && npm run dev

# Criar nova migration
cd backend/src/Infrastructure && dotnet ef migrations add NomeDaMigration
```