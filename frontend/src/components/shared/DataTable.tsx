'use client';

import { ArrowDown, ArrowUp, ArrowUpDown, ChevronLeft, ChevronRight } from 'lucide-react';
import { useId, type ReactNode } from 'react';

import { useI18n } from '@/components/providers/LanguageProvider';
import { Button } from '@/components/ui/Button';
import type { SortState } from '@/lib/api/pagination';
import { cn } from '@/lib/utils/cn';
import { formatCount } from '@/lib/utils/format';

import { TableSkeleton } from './Skeletons';

type Align = 'left' | 'right' | 'center';

export interface DataTableColumn<TRow> {
  id: string;
  header: string;
  cell: (row: TRow) => ReactNode;
  sortable?: boolean;
  align?: Align;
  className?: string;
  headerClassName?: string;
}

export interface DataTablePagination {
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

type Props<TRow> = {
  data: readonly TRow[];
  columns: readonly DataTableColumn<TRow>[];
  getRowId: (row: TRow) => string;
  caption: string;
  isLoading?: boolean;
  sort?: SortState | null;
  onSortChange?: (sort: SortState) => void;
  pagination?: DataTablePagination;
  onPageChange?: (page: number) => void;
  toolbar?: ReactNode;
  emptyState?: ReactNode;
  className?: string;
};

const ALIGN_CLASSES: Record<Align, string> = {
  left: 'text-left',
  right: 'text-right',
  center: 'text-center',
};

function nextSort(current: SortState | null | undefined, field: string): SortState {
  if (current?.field === field) {
    return { field, direction: current.direction === 'asc' ? 'desc' : 'asc' };
  }

  return { field, direction: 'asc' };
}

/**
 * Shared data grid (DESIGN.md §2.5). Server-side pagination and sorting are
 * controlled by the caller so TanStack Query remains the single source of
 * server state. Numeric columns opt into right alignment + tabular numerals.
 */
export function DataTable<TRow>({
  data,
  columns,
  getRowId,
  caption,
  isLoading = false,
  sort,
  onSortChange,
  pagination,
  onPageChange,
  toolbar,
  emptyState,
  className,
}: Props<TRow>) {
  const captionId = useId();
  const { t } = useI18n();

  if (isLoading) {
    return <TableSkeleton columns={columns.length} />;
  }

  if (data.length === 0) {
    return (
      <div className={cn('rounded-lg border border-line bg-surface', className)}>
        {emptyState ?? (
          <p className="px-6 py-14 text-center text-sm text-charcoal-light">{t('table.empty')}</p>
        )}
      </div>
    );
  }

  const totalItems = pagination?.totalItems ?? data.length;
  const page = pagination?.page ?? 1;
  const pageSize = pagination?.pageSize ?? data.length;
  const totalPages = Math.max(pagination?.totalPages ?? 1, 1);
  const firstRow = totalItems === 0 ? 0 : (page - 1) * pageSize + 1;
  const lastRow = Math.min(page * pageSize, totalItems);

  return (
    <div
      className={cn(
        'overflow-hidden rounded-lg border border-line bg-surface shadow-sm',
        className,
      )}
    >
      {toolbar ? <div className="border-b border-line px-4 py-3">{toolbar}</div> : null}

      <div className="overflow-x-auto">
        <table aria-labelledby={captionId} className="w-full min-w-[640px] border-collapse text-sm">
          <caption id={captionId} className="sr-only">
            {caption}
          </caption>
          <thead className="bg-surface-muted/70">
            <tr>
              {columns.map((column) => {
                const align = column.align ?? 'left';
                const isSorted = sort?.field === column.id;
                const ariaSort = !column.sortable
                  ? undefined
                  : isSorted
                    ? sort.direction === 'asc'
                      ? 'ascending'
                      : 'descending'
                    : 'none';

                return (
                  <th
                    key={column.id}
                    scope="col"
                    aria-sort={ariaSort}
                    className={cn(
                      'whitespace-nowrap border-b border-line px-4 py-3 text-xs font-semibold uppercase tracking-wide text-charcoal-light',
                      ALIGN_CLASSES[align],
                      column.headerClassName,
                    )}
                  >
                    {column.sortable && onSortChange ? (
                      <button
                        type="button"
                        onClick={() => onSortChange(nextSort(sort, column.id))}
                        aria-label={t(
                          isSorted && sort?.direction === 'asc'
                            ? 'table.sort.descending'
                            : 'table.sort.ascending',
                          { column: column.header },
                        )}
                        className={cn(
                          'inline-flex items-center gap-1 rounded-sm transition-colors duration-300 hover:text-charcoal focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary',
                          align === 'right' && 'w-full justify-end',
                          align === 'center' && 'w-full justify-center',
                        )}
                      >
                        {column.header}
                        {isSorted ? (
                          sort?.direction === 'asc' ? (
                            <ArrowUp className="h-3.5 w-3.5" aria-hidden="true" />
                          ) : (
                            <ArrowDown className="h-3.5 w-3.5" aria-hidden="true" />
                          )
                        ) : (
                          <ArrowUpDown className="h-3.5 w-3.5 opacity-60" aria-hidden="true" />
                        )}
                      </button>
                    ) : (
                      column.header
                    )}
                  </th>
                );
              })}
            </tr>
          </thead>
          <tbody>
            {data.map((row) => (
              <tr
                key={getRowId(row)}
                className="border-b border-line/60 transition-colors duration-300 last:border-b-0 hover:bg-surface-muted/50"
              >
                {columns.map((column) => (
                  <td
                    key={column.id}
                    className={cn(
                      'border-b border-line/40 px-4 py-3 align-middle text-charcoal',
                      ALIGN_CLASSES[column.align ?? 'left'],
                      column.className,
                    )}
                  >
                    {column.cell(row)}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {pagination ? (
        <nav
          aria-label={t('table.pagination.label')}
          className="flex flex-col gap-3 border-t border-line px-4 py-3 sm:flex-row sm:items-center sm:justify-between"
        >
          <p className="text-xs tabular-nums text-charcoal-light">
            {t('table.pagination.showing', {
              from: formatCount(firstRow),
              to: formatCount(lastRow),
              total: formatCount(totalItems),
            })}
          </p>
          <div className="flex items-center gap-2">
            <Button
              variant="secondary"
              size="sm"
              disabled={page <= 1}
              onClick={() => onPageChange?.(page - 1)}
              aria-label={t('table.pagination.previous')}
            >
              <ChevronLeft className="h-4 w-4" aria-hidden="true" />
              <span className="hidden sm:inline">{t('table.pagination.previous')}</span>
            </Button>
            <span className="text-xs tabular-nums text-charcoal-light">
              {t('table.pagination.page', { page, totalPages })}
            </span>
            <Button
              variant="secondary"
              size="sm"
              disabled={page >= totalPages}
              onClick={() => onPageChange?.(page + 1)}
              aria-label={t('table.pagination.next')}
            >
              <span className="hidden sm:inline">{t('table.pagination.next')}</span>
              <ChevronRight className="h-4 w-4" aria-hidden="true" />
            </Button>
          </div>
        </nav>
      ) : null}
    </div>
  );
}
