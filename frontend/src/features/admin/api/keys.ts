import type { ListQueryParams } from '@/lib/api/pagination';
import { QUERY_KEY_ROOTS } from '@/lib/api/query-keys';

export interface AuditLogQueryParams extends ListQueryParams {
  action?: string;
  moduleCode?: string;
}

export const adminKeys = {
  all: [QUERY_KEY_ROOTS.admin] as const,
  tenants: () => [...adminKeys.all, 'tenants'] as const,
  tenantList: (params: ListQueryParams) => [...adminKeys.tenants(), 'list', params] as const,
  auditLogs: () => [...adminKeys.all, 'audit-logs'] as const,
  auditLogList: (params: AuditLogQueryParams) =>
    [...adminKeys.auditLogs(), 'list', params] as const,
  sessions: () => [...adminKeys.all, 'sessions'] as const,
  sessionList: (params: ListQueryParams) => [...adminKeys.sessions(), 'list', params] as const,
};
