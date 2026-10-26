import type { Session } from '@/lib/auth/types';
import type { PagedResult } from '@/lib/api/pagination';

/** Builds a valid session with deterministic tenant/user metadata. */
export function createSession(overrides: Partial<Session> = {}): Session {
  return {
    user: { id: 'user-1', email: 'qa@omnimanage.test', roles: ['tenant-admin'] },
    tenant: { id: 'tenant-1', code: 'ACME', name: 'Acme Trading' },
    activeModules: [],
    permissions: [],
    ...overrides,
  };
}

/** Wraps rows in the documented `PagedResult<T>` envelope. */
export function pageOf<T>(
  items: readonly T[],
  overrides: Partial<PagedResult<T>> = {},
): PagedResult<T> {
  return {
    items: [...items],
    page: 1,
    pageSize: 10,
    totalItems: items.length,
    totalPages: items.length > 0 ? 1 : 0,
    ...overrides,
  };
}
