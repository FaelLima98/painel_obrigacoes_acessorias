---
name: new-feature
description: >
  Use para criar uma nova feature completa no frontend React. Cria a estrutura
  de pastas, componentes, hooks TanStack Query e tipos TypeScript seguindo o
  padrão feature-based do projeto. Invoque com /new-feature <nome-da-feature>.
allowed-tools: Read, Edit, Bash(cd frontend && npm run type-check *)
disable-model-invocation: true
---

# Skill: Nova Feature Frontend

## Argumento
Nome da feature em kebab-case (ex: `companies`, `calendar`, `alerts`).

## Estrutura a criar

```
frontend/src/features/<nome>/
├── components/
│   └── <Nome>Table.tsx       # componente principal de listagem
├── hooks/
│   └── use<Nome>.ts          # hooks TanStack Query
├── types.ts                  # tipos TypeScript da feature
└── index.ts                  # re-exports públicos
```

## Template: hook TanStack Query

```typescript
// hooks/use<Nome>.ts
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/shared/services/apiClient';
import type { <Entidade>Response, Create<Entidade>Dto } from '../types';

const QUERY_KEY = ['<nome>'] as const;

export function use<Nome>() {
  return useQuery({
    queryKey: QUERY_KEY,
    queryFn: () =>
      apiClient.get<{ data: <Entidade>Response[] }>('/<rota>').then(r => r.data.data),
  });
}

export function useCreate<Entidade>() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: Create<Entidade>Dto) =>
      apiClient.post('/<rota>', payload).then(r => r.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEY });
    },
    onError: (error: AxiosError) => {
      // tratar 409 (conflito) separadamente se necessário
    },
  });
}
```

## Template: componente com Ant Design

```typescript
// components/<Nome>Table.tsx
import { Button, Popconfirm, Table } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { use<Nome>, useDelete<Entidade> } from '../hooks/use<Nome>';
import type { <Entidade>Response } from '../types';

export function <Nome>Table() {
  const { data, isLoading } = use<Nome>();
  const deleteMutation = useDelete<Entidade>();

  const columns: ColumnsType<<Entidade>Response> = [
    { title: 'Nome', dataIndex: 'nome', key: 'nome' },
    {
      title: 'Ações',
      key: 'acoes',
      render: (_, record) => (
        <Popconfirm
          title="Confirmar exclusão?"
          onConfirm={() => deleteMutation.mutate(record.id)}
        >
          <Button danger size="small">Remover</Button>
        </Popconfirm>
      ),
    },
  ];

  return (
    <Table
      columns={columns}
      dataSource={data}
      rowKey="id"
      loading={isLoading}
    />
  );
}
```

## Padrões obrigatórios

### Feedback ao usuário (Ant Design)
```typescript
import { App } from 'antd';
const { message, notification } = App.useApp();

// Sucesso
message.success('Empresa cadastrada com sucesso.');

// Erro de conflito (409)
if (error.response?.status === 409) {
  message.error('CNPJ já cadastrado.');
} else {
  notification.error({ message: 'Erro inesperado', description: '...' });
}
```

### TypeScript
- Nunca usar `any`
- Tipar todos os responses da API
- Props de componentes com interface explícita

### Status Badge (componente compartilhado)
```typescript
import { StatusBadge } from '@/shared/components/StatusBadge';
// Props: status: 'Pendente' | 'Atrasada' | 'Entregue' | 'NaoAplicavel'
```

## Após criar a feature
1. Adicionar a rota em `App.tsx`
2. Adicionar item no menu lateral (Ant Design Sider)
3. Rodar `npm run type-check` e corrigir erros antes de continuar
4. Docimentar a feature no README do frontend (se necessário)