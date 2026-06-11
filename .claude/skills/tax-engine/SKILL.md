---
name: tax-engine
description: >
  Use quando precisar implementar, modificar ou corrigir a TaxObligationEngine —
  a engine de regras tributárias que calcula obrigações acessórias por regime.
  Auto-invocada ao trabalhar em Domain/Services, nos testes unitários da engine,
  ou ao discutir regras de vencimento e periodicidade de obrigações.
allowed-tools: Read, Edit, Bash(dotnet test *), Bash(dotnet build *)
---

# Skill: TaxObligationEngine

## Contexto
A `TaxObligationEngine` é o coração do domínio. Ela é uma classe **pura e sem I/O**
que recebe `(RegimeTributario regime, int year, int month)` e devolve
`IEnumerable<ObrigacaoCalculada>`.

Obrigações **não** são persistidas no banco. Apenas `Entregas` são salvas.
O backend cruza o resultado da engine com as entregas do banco para calcular o status.

## Estrutura esperada

```csharp
// Domain/Services/TaxObligationEngine.cs
public class TaxObligationEngine
{
    public IEnumerable<ObrigacaoCalculada> Calculate(
        RegimeTributario regime, int year, int month)
    {
        // Retorna lista vazia para Imunidade/Isenção
        // Obrigações anuais só aparecem quando month == 1
        // Cada obrigação tem: Tipo, Vencimento, Periodicidade
    }

    private DateOnly CalculateVencimento(TipoObrigacao tipo, int year, int month) { ... }

    private DateOnly ProximoDiaUtil(DateOnly date)
    {
        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            date = date.AddDays(1);
        return date;
    }
}
```

## Tabela de obrigações × regime
Consulte o CLAUDE.md para a tabela completa. Pontos críticos:
- **Imunidade/Isenção** → nenhuma obrigação
- **eSocial, DIRF, RAIS** → aplicam-se a TODOS os regimes (exceto Imunidade)
- **Anuais** → APENAS quando `month == 1`

## Regras de vencimento críticas
- **DAS**: `dia 20 do mês seguinte` — aplicar `ProximoDiaUtil()` se cair em fim de semana
- **DCTF**: `dia 15 do SEGUNDO mês seguinte` (ex: competência março → vence 15/maio)
- **Anuais**: vencimentos fixos no ano seguinte ao exercício (ver tabela no CLAUDE.md)

## Erros comuns a evitar
1. DCTF com delay de 1 mês (deve ser 2)
2. Obrigações anuais aparecendo em todos os meses (só em janeiro)
3. ProximoDiaUtil não implementado para o DAS
4. eSocial não incluído no Simples Nacional

## Cobertura de testes obrigatória (xUnit)
Para cada cenário abaixo, verificar tipos corretos E vencimentos corretos:
- Simples Nacional em mês comum (ex: março)
- Simples Nacional em janeiro (deve incluir anuais)
- Lucro Presumido em mês comum
- Lucro Presumido em janeiro (deve incluir anuais)
- Lucro Real em mês comum
- Imunidade/Isenção (deve retornar lista vazia)
- DAS cujo dia 20 cai em sábado → verificar prorrogação
- DAS cujo dia 20 cai em domingo → verificar prorrogação
- DCTF: competência março → vencimento 15/maio

## Workflow ao modificar a engine
1. Leia o arquivo atual de `TaxObligationEngine.cs`
2. Implemente a alteração
3. Rode `dotnet test` e mostre o resultado
4. Se falhar, corrija antes de continuar