---
name: check-and-test
description: >
  Use para rodar os testes unitários do backend, verificar cobertura da engine
  tributária e validar que as regras de negócio estão corretas. Invoque com
  /check-and-test a qualquer momento para garantir que nada quebrou.
allowed-tools: Bash(dotnet test *), Bash(dotnet build *), Read
disable-model-invocation: true
---

# Skill: Check & Test

## O que este skill faz

1. Compila o backend
2. Roda os testes unitários com output detalhado
3. Verifica se os casos críticos estão cobertos
4. Reporta o que está faltando

## Execução

```bash
cd backend
dotnet build --no-restore 2>&1
dotnet test --verbosity normal 2>&1
```

## Casos obrigatórios — a cobertura está completa quando todos passam

### TaxObligationEngine
- [ ] Simples Nacional — mês comum: DAS + eSocial (sem anuais)
- [ ] Simples Nacional — janeiro: DAS + eSocial + DEFIS + DIRF + RAIS
- [ ] Lucro Presumido — mês comum: DCTF + EFD-ICMS + EFD Contrib + EFD-Reinf + eSocial
- [ ] Lucro Presumido — janeiro: acima + SPED ECD + SPED ECF + DIRF + RAIS
- [ ] Lucro Real — igual Lucro Presumido (verificar separadamente)
- [ ] Imunidade/Isenção — qualquer mês: lista vazia

### Vencimentos
- [ ] DAS competência Jan/2025 (dia 20/02/2025 = quinta → sem prorrogação)
- [ ] DAS competência que cai em sábado → prorrogado para segunda
- [ ] DAS competência que cai em domingo → prorrogado para segunda
- [ ] DCTF competência março → vence 15/maio (não 15/abril!)
- [ ] SPED ECD exercício 2024 → vence 31/maio/2025
- [ ] SPED ECF exercício 2024 → vence 31/julho/2025
- [ ] DIRF exercício 2024 → vence 28/fev/2025 (último dia de fev)
- [ ] RAIS/DEFIS exercício 2024 → vence 31/mar/2025

## Como reportar
Após rodar os testes, liste:
- ✅ Casos passando
- ❌ Casos falhando (com mensagem de erro)
- ⚠️ Casos não cobertos (sem teste existente)

Se houver falhas, implemente a correção antes de continuar.