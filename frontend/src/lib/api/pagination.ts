/** Mirrors the backend `PagedResult<T>` contract (Rules.md §5.2). */
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

/** Server-enforced page size cap from Rules.md §5.2. */
export const MAX_PAGE_SIZE = 100;

export interface ListQueryParams {
  page?: number;
  pageSize?: number;
  search?: string;
  sort?: string;
}

export type SortDirection = 'asc' | 'desc';

export interface SortState {
  field: string;
  direction: SortDirection;
}

/** Converts table state into the documented `-field` descending syntax. */
export function toSortParam(sort: SortState): string {
  return sort.direction === 'desc' ? `-${sort.field}` : sort.field;
}

export type QueryValue = string | number | boolean | null | undefined;

function isSerializableQueryValue(value: unknown): value is string | number | boolean {
  return typeof value === 'string' || typeof value === 'number' || typeof value === 'boolean';
}

/**
 * Builds a query string, omitting empty values so URLs and TanStack Query keys
 * stay canonical (no `?search=` noise when a filter is cleared).
 */
export function toQueryString(params: Readonly<object>): string {
  const search = new URLSearchParams();

  for (const [key, value] of Object.entries(params)) {
    if (!isSerializableQueryValue(value) || value === '') {
      continue;
    }

    search.set(key, String(value));
  }

  const query = search.toString();

  return query.length > 0 ? `?${query}` : '';
}
