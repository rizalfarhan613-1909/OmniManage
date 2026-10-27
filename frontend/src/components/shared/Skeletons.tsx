'use client';

import { useI18n } from '@/components/providers/LanguageProvider';
import { cn } from '@/lib/utils/cn';

type SkeletonProps = {
  className?: string;
};

/** Base Sage loading block (DESIGN.md §7). */
export function Skeleton({ className }: SkeletonProps) {
  return (
    <div
      aria-hidden="true"
      className={cn('animate-pulse rounded-md bg-surface-muted', className)}
    />
  );
}

type CountProps = {
  count?: number;
};

export function KpiGridSkeleton({ count = 4 }: CountProps) {
  const { t } = useI18n();

  return (
    <div role="status" aria-live="polite" className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
      <span className="sr-only">{t('states.loading')}</span>
      {Array.from({ length: count }, (_, index) => (
        <div
          key={`kpi-${index}`}
          className="rounded-lg border border-line bg-surface p-5 shadow-sm"
        >
          <Skeleton className="h-4 w-28" />
          <Skeleton className="mt-4 h-9 w-44" />
          <Skeleton className="mt-3 h-3 w-36" />
        </div>
      ))}
    </div>
  );
}

type TableSkeletonProps = {
  columns?: number;
  rows?: number;
};

export function TableSkeleton({ columns = 5, rows = 6 }: TableSkeletonProps) {
  const { t } = useI18n();

  return (
    <div
      role="status"
      aria-live="polite"
      className="overflow-hidden rounded-lg border border-line bg-surface"
    >
      <span className="sr-only">{t('states.loading')}</span>
      <div className="flex gap-4 border-b border-line bg-surface-muted/60 px-4 py-3">
        {Array.from({ length: columns }, (_, index) => (
          <Skeleton key={`head-${index}`} className="h-3 flex-1" />
        ))}
      </div>
      {Array.from({ length: rows }, (_, rowIndex) => (
        <div
          key={`row-${rowIndex}`}
          className="flex gap-4 border-b border-line/60 px-4 py-4 last:border-b-0"
        >
          {Array.from({ length: columns }, (_, columnIndex) => (
            <Skeleton key={`cell-${rowIndex}-${columnIndex}`} className="h-4 flex-1" />
          ))}
        </div>
      ))}
    </div>
  );
}

export function ChartSkeleton() {
  const { t } = useI18n();

  return (
    <div
      role="status"
      aria-live="polite"
      className="rounded-lg border border-line bg-surface p-5 shadow-sm"
    >
      <span className="sr-only">{t('states.loading')}</span>
      <Skeleton className="h-4 w-40" />
      <Skeleton className="mt-2 h-3 w-64" />
      <Skeleton className="mt-6 h-72 w-full" />
    </div>
  );
}

type ListSkeletonProps = {
  rows?: number;
};

export function ListSkeleton({ rows = 4 }: ListSkeletonProps) {
  const { t } = useI18n();

  return (
    <div role="status" aria-live="polite" className="space-y-3">
      <span className="sr-only">{t('states.loading')}</span>
      {Array.from({ length: rows }, (_, index) => (
        <div
          key={`list-${index}`}
          className="flex items-center gap-3 rounded-lg border border-line bg-surface p-4"
        >
          <Skeleton className="h-9 w-9 rounded-full" />
          <div className="flex-1 space-y-2">
            <Skeleton className="h-4 w-2/3" />
            <Skeleton className="h-3 w-1/3" />
          </div>
        </div>
      ))}
    </div>
  );
}

export function DashboardSkeleton() {
  return (
    <div className="space-y-6">
      <KpiGridSkeleton />
      <div className="grid gap-6 xl:grid-cols-3">
        <div className="xl:col-span-2">
          <ChartSkeleton />
        </div>
        <ListSkeleton rows={5} />
      </div>
    </div>
  );
}
