import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import type { UseMutationResult, UseQueryResult } from '@tanstack/react-query';
import type { ReactNode } from 'react';
import { vi } from 'vitest';

export function createTestQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: Infinity },
      mutations: { retry: false },
    },
  });
}

export function createQueryWrapper(queryClient: QueryClient) {
  return function QueryWrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
  };
}

/** Minimal query result for view tests; only the fields the view reads. */
export function queryResult<TData>(
  overrides: Partial<UseQueryResult<TData, Error>> = {},
): UseQueryResult<TData, Error> {
  return {
    data: undefined,
    error: null,
    isLoading: false,
    refetch: vi.fn(),
    ...overrides,
  } as unknown as UseQueryResult<TData, Error>;
}

/** Minimal mutation result for interaction tests. */
export function mutationResult<TData, TVariables>(
  overrides: Record<string, unknown> = {},
): UseMutationResult<TData, Error, TVariables, unknown> {
  return {
    mutate: vi.fn(),
    reset: vi.fn(),
    isPending: false,
    error: null,
    ...overrides,
  } as unknown as UseMutationResult<TData, Error, TVariables, unknown>;
}
