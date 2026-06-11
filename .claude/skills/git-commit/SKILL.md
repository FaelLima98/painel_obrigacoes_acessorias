---
name: git-commit
description: >
  Use para criar um commit git com mensagem semântica adequada ao projeto.
  Verifica status, exibe diff resumido e propõe mensagem de commit no formato
  convencional antes de confirmar.
allowed-tools: Bash(git status), Bash(git diff *), Bash(git add *), Bash(git commit *)
disable-model-invocation: true
---

# Skill: Git Commit

## Argumentos
Descrição opcional do que foi feito (ex: `implementei a TaxObligationEngine`).
Se não fornecida, o skill analisa o diff para propor a mensagem.

## Workflow

### 1. Verificar estado atual
```bash
git status
git diff --stat HEAD
```

### 2. Formato de mensagem (Conventional Commits)
```
<tipo>(<escopo>): <descrição curta em português>

[corpo opcional com mais detalhes]
```

**Tipos:**
- `feat` — nova funcionalidade
- `fix` — correção de bug
- `test` — adição/correção de testes
- `refactor` — refatoração sem mudança funcional
- `docs` — documentação
- `chore` — configuração, docker, ci

**Escopos comuns neste projeto:**
- `domain` — TaxObligationEngine, entidades, enums
- `infra` — EF Core, migrations, seed
- `api` — endpoints, Program.cs
- `frontend` — componentes React, hooks
- `docker` — Docker Compose, Dockerfiles

### 3. Exemplos de mensagens boas
```
feat(domain): implementa TaxObligationEngine com regras por regime
feat(api): adiciona endpoints de empresas e obrigações
fix(domain): corrige cálculo DCTF para usar delay de 2 meses
test(domain): cobre casos de prorrogação de fim de semana no DAS
feat(frontend): implementa calendário de obrigações com Ant Design
feat(infra): adiciona seed automático com 8 empresas demo
```

### 4. Antes de commitar
- Verificar se `dotnet build` passa (backend)
- Verificar se não há `console.log` esquecidos (frontend)
- Não commitar `.env` com credenciais reais

### 5. Executar commit
Propor a mensagem ao usuário e aguardar confirmação antes de executar:
```bash
git add -A
git commit -m "<mensagem proposta>"
```