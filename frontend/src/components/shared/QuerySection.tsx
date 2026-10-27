'use client';

import type { UseQueryResult } from '@tanstack/react-query';
import type { ReactNode } from 'react';

import { useI18n } from '@/components/providers/LanguageProvider';
import { ApiErrorState } from '@/components/shared/ApiErrorState';
import { EmptyState } from '@/components/shared/EmptyState';
import { Button } from '@/components/ui/Button';

type Props<TData> = {
  query: Pick<UseQueryResult<TData, Error>, 'isLoading' | 'error' | 'data' | 'refetch'>;
  skeleton: ReactNode;
  isEmpty: (data: TData) => boolean;
  emptyTitle: string;
  emptyDescription: string;
  emptyActionLabel?: string;
  moduleCode?: string;
  children: (data: TData) => ReactNode;
};

/**
 * Enforces the five required UI states for a query-backed screen
 * (DESIGN.md §7): loading, error, empty, forbidden and not-licensed. The last
 * two are produced by `ApiErrorState` from the typed API error.
 */
export function QuerySection<TData>({
  query,
  skeleton,
  isEmpty,
  emptyTitle,
  emptyDescription,
  emptyActionLabel,
  moduleCode,
  children,
}: Props<TData>) {
  const { t } = useI18n();

  if (query.isLoading) {
    return <>{skeleton}</>;
  }

  if (query.error) {
    return (
      <ApiErrorState
        error={query.error}
        moduleCode={moduleCode}
        onRetry={() => void query.refetch()}
      />
    );
  }

  if (query.data === undefined || query.data === null || isEmpty(query.data)) {
    return (
      <EmptyState
        title={emptyTitle}
        description={emptyDescription}
        action={
          <Button onClick={() => void query.refetch()}>
            {emptyActionLabel ?? t('common.refresh')}
          </Button>
        }
      />
    );
  }

  return <>{children(query.data)}</>;
}
