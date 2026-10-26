import { fireEvent, render, screen, within } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import type { ActiveUserSession, AuditLogEntry } from '@/features/admin/api/types';
import { API_ERROR_CODES, ApiRequestError } from '@/lib/api/errors';
import { pageOf } from '@/test/fixtures';
import { mutationResult, queryResult } from '@/test/query';

import { AuditSecurityView } from './AuditSecurityView';

const api = vi.hoisted(() => ({
  useAuditLogs: vi.fn(),
  useActiveSessions: vi.fn(),
  useTerminateSession: vi.fn(),
}));

vi.mock('@/features/admin/api/use-audit-logs', () => ({
  useAuditLogs: api.useAuditLogs,
}));

vi.mock('@/features/admin/api/use-active-sessions', () => ({
  useActiveSessions: api.useActiveSessions,
}));

vi.mock('@/features/admin/api/use-terminate-session', () => ({
  useTerminateSession: api.useTerminateSession,
}));

const AUDIT_ENTRY: AuditLogEntry = {
  id: 'audit-1',
  occurredAt: '2026-10-21T08:15:30Z',
  actor: {
    id: 'user-42',
    name: 'Sari Admin',
    type: 'Superadmin',
  },
  action: 'platform.module.activated',
  moduleCode: 'SALES_CORE',
  entityType: 'TenantModule',
  entityId: 'tenant-1:SALES_CORE',
  ipAddress: '10.0.0.8',
  userAgent: 'Mozilla/5.0',
  traceId: 'trace-audit-1',
  beforeData: { status: 'Draft' },
  afterData: { status: 'Confirmed' },
  rowHash: 'abcdef1234567890abcdef1234567890',
  prevHash: '00000000000000000000000000000000',
};

const ACTIVE_SESSION: ActiveUserSession = {
  id: 'session-1',
  userId: 'user-7',
  userName: 'Rina Putri',
  email: 'rina@acme.test',
  roles: ['TENANT_ADMIN'],
  ipAddress: '203.0.113.5',
  userAgent: 'Chrome on Windows',
  startedAt: '2026-10-21T07:00:00Z',
  lastSeenAt: '2026-10-21T08:30:00Z',
  status: 'Active',
};

function renderView() {
  return render(<AuditSecurityView />);
}

beforeEach(() => {
  api.useAuditLogs.mockReturnValue(queryResult({ data: pageOf([AUDIT_ENTRY]) }));
  api.useActiveSessions.mockReturnValue(queryResult({ data: pageOf([ACTIVE_SESSION]) }));
  api.useTerminateSession.mockReturnValue(mutationResult({ mutate: vi.fn() }));
});

describe('AuditSecurityView', () => {
  it('renders the immutable audit trail with actor, action, module, IP and hash', () => {
    renderView();

    expect(screen.getByText('Sari Admin')).toBeInTheDocument();
    expect(screen.getByText('Superadmin')).toBeInTheDocument();
    expect(screen.getByText('platform.module.activated')).toBeInTheDocument();
    expect(screen.getByText('SALES_CORE')).toBeInTheDocument();
    expect(screen.getByText('10.0.0.8')).toBeInTheDocument();
    expect(screen.getByLabelText('Row hash abcdef1234567890abcdef1234567890')).toBeInTheDocument();
    expect(screen.getByText('Hash-chained append-only')).toBeInTheDocument();
  });

  it('renders the before/after payload diff', () => {
    renderView();

    expect(screen.getByText('View diff')).toBeInTheDocument();
    expect(screen.getByText('(1 changed fields)')).toBeInTheDocument();
    expect(screen.getByText('status')).toBeInTheDocument();
    expect(screen.getByText('Draft')).toBeInTheDocument();
    expect(screen.getByText('Confirmed')).toBeInTheDocument();
  });

  it('renders active sessions and terminates a session after confirmation', () => {
    const mutate = vi.fn((_sessionId: string, options?: { onSuccess?: () => void }) => {
      options?.onSuccess?.();
    });
    api.useTerminateSession.mockReturnValue(mutationResult({ mutate }));

    renderView();

    expect(screen.getByText('Rina Putri')).toBeInTheDocument();
    expect(screen.getByText('rina@acme.test')).toBeInTheDocument();
    expect(screen.getByText('TENANT_ADMIN')).toBeInTheDocument();
    expect(screen.getByText('203.0.113.5')).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Terminate session for Rina Putri' }));

    const dialog = screen.getByRole('dialog');
    expect(
      within(dialog).getByRole('heading', { name: "Terminate Rina Putri's session?" }),
    ).toBeInTheDocument();

    fireEvent.click(within(dialog).getByRole('button', { name: 'Terminate session' }));

    expect(mutate).toHaveBeenCalledWith(
      'session-1',
      expect.objectContaining({
        onSuccess: expect.any(Function),
      }),
    );
    expect(screen.getByText('Session for Rina Putri was terminated.')).toBeInTheDocument();
  });

  it('renders the audit loading skeleton', () => {
    api.useAuditLogs.mockReturnValue(queryResult({ isLoading: true }));

    renderView();

    expect(screen.getByRole('status')).toHaveTextContent('Loading content');
  });

  it('renders both empty states', () => {
    api.useAuditLogs.mockReturnValue(queryResult({ data: pageOf([]) }));
    api.useActiveSessions.mockReturnValue(queryResult({ data: pageOf([]) }));

    renderView();

    expect(screen.getByText('No audit events yet')).toBeInTheDocument();
    expect(screen.getByText('No active sessions')).toBeInTheDocument();
  });

  it('renders the audit error state with a copyable traceId', () => {
    api.useAuditLogs.mockReturnValue(
      queryResult({
        error: new ApiRequestError({
          code: API_ERROR_CODES.unexpected,
          message: 'Audit trail unavailable.',
          status: 500,
          traceId: 'trace-audit-500',
        }),
      }),
    );

    renderView();

    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    expect(screen.getByText('trace-audit-500')).toBeInTheDocument();
  });
});
