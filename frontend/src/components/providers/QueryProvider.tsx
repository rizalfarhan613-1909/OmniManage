'use client';

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useState, type ReactNode } from 'react';

import { ApiRequestError } from '@/lib/api/errors';

type Props = {
  children: ReactNode;
};

/**
 * Server-state provider for the authenticated shell (Rules.md §10.1).
 *
 * Client errors (4xx) are never retried: a 403 license/permission denial is
 * authoritative and retrying only adds noise. Transient failures get two
 * retries with TanStack Query's exponential backoff.
 */
export function QueryProvider({ children }: Props) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 30_000,
            gcTime: 5 * 60_000,
            refetchOnWindowFocus: false,
            retry: (failureCount, error) => {
              if (error instanceof ApiRequestError && error.status >= 400 && error.status < 500) {
                return false;
              }

              return failureCount < 2;
            },
          },
          mutations: {
            retry: false,
          },
        },
      }),
  );

  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}
