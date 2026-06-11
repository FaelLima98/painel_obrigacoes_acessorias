---
name: new-endpoint
description: >
  Use para criar um novo endpoint Minimal API no backend, seguindo o padrão
  de organização do projeto. Invoque com /new-endpoint descrevendo o endpoint
  desejado (ex: /new-endpoint GET /api/alerts com filtro por diasAdiante).
allowed-tools: Read, Edit, Bash(dotnet build *)
disable-model-invocation: true
---

# Skill: Novo Endpoint Minimal API

## Argumentos esperados
Descreva o endpoint: método HTTP, rota, o que recebe e o que retorna.

## Checklist de implementação

### 1. Criar o arquivo de endpoints
Local: `backend/src/API/Endpoints/<Dominio>Endpoints.cs`

```csharp
namespace EAuditoria.API.Endpoints;

public static class <Dominio>Endpoints
{
    public static IEndpointRouteBuilder Map<Dominio>Endpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/<rota>")
            .WithTags("<Dominio>")
            .WithOpenApi();

        group.MapGet("/", GetAll).WithName("Get<Dominio>List");
        // adicionar demais verbos conforme necessário

        return app;
    }

    private static async Task<IResult> GetAll(
        // injetar dependências via parâmetros (DI automático no Minimal API)
        I<Dominio>Repository repo,
        CancellationToken ct)
    {
        var result = await repo.GetAllAsync(ct);
        return Results.Ok(result);
    }
}
```

### 2. Registrar em Program.cs
```csharp
app.Map<Dominio>Endpoints();
```

### 3. Padrões de resposta obrigatórios
| Situação | Retorno |
|---|---|
| Sucesso com dados | `Results.Ok(data)` |
| Criado | `Results.Created($"/api/rota/{id}", data)` |
| Não encontrado | `Results.NotFound()` |
| Conflito (CNPJ duplicado, entrega duplicada) | `Results.Conflict(problem)` |
| Validação | `Results.ValidationProblem(errors)` |
| Erro inesperado | `Results.Problem(...)` |

### 4. DTOs
Criar em `Application/DTOs/<Dominio>/`:
- `<Entidade>Request.cs` — para receber dados do cliente
- `<Entidade>Response.cs` — para retornar dados ao cliente
- Nunca expor entidades de domínio diretamente na API

### 5. Validações de negócio comuns
- CNPJ duplicado → buscar por CNPJ antes de inserir → 409 se existir
- Entrega duplicada → índice único no banco + tratar DbUpdateException → 409
- IDs inválidos (GUID) → .NET já valida na binding, retorna 400 automaticamente

## Após criar o endpoint
1. Verificar se `dotnet build` passa sem warnings
2. Testar manualmente via curl ou Swagger (http://localhost:5000/swagger)
3. Documentar no README se for endpoint relevante