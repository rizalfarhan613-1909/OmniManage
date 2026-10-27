'use client';

import { useI18n } from '@/components/providers/LanguageProvider';

import { CheckCircle2, ShieldCheck } from 'lucide-react';
import { useState } from 'react';

import { ConfirmDialog } from '@/components/shared/ConfirmDialog';
import { DataTable, type DataTableColumn } from '@/components/shared/DataTable';
import { PageHeader } from '@/components/shared/PageHeader';
import { QuerySection } from '@/components/shared/QuerySection';
import { TableSkeleton } from '@/components/shared/Skeletons';
import { StatusBadge, type StatusTone } from '@/components/shared/StatusBadge';
import { Button } from '@/components/ui/Button';
import { getModuleLabel } from '@/config/module-labels';
import { useActiveSessions } from '@/features/admin/api/use-active-sessions';
import { useAuditLogs } from '@/features/admin/api/use-audit-logs';
import { useTerminateSession } from '@/features/admin/api/use-terminate-session';
import type {
  ActiveSessionStatus,
  ActiveUserSession,
  AuditActorType,
  AuditLogEntry,
} from '@/features/admin/api/types';
import { toSortParam, type SortState } from '@/lib/api/pagination';
import type { TranslationKey } from '@/lib/i18n';
import { formatDateTime } from '@/lib/utils/format';

import { PayloadDiff } from './PayloadDiff';

const PAGE_SIZE = 10;

const ACTOR_TYPE_LABEL: Record<AuditActorType, TranslationKey> = {
  User: 'admin.security.audit.actorType.user',
  System: 'admin.security.audit.actorType.system',
  Superadmin: 'admin.security.audit.actorType.superadmin',
  Support: 'admin.security.audit.actorType.support',
};

const SESSION_STATUS_TONE: Record<ActiveSessionStatus, StatusTone> = {
  Active: 'success',
  Idle: 'neutral',
};

const SESSION_STATUS_LABEL: Record<ActiveSessionStatus, TranslationKey> = {
  Active: 'admin.security.sessions.status.active',
  Idle: 'admin.security.sessions.status.idle',
};

function sessionUserLabel(session: ActiveUserSession, unknownLabel: string): string {
  return session.userName || session.email || unknownLabel;
}

export function AuditSecurityView() {
  const { t } = useI18n();
  const [page, setPage] = useState(1);
  const [sessionPage, setSessionPage] = useState(1);
  const [sort, setSort] = useState<SortState>({
    field: 'occurredAt',
    direction: 'desc',
  });
  const [sessionToTerminate, setSessionToTerminate] = useState<ActiveUserSession | null>(null);
  const [announcement, setAnnouncement] = useState<string | null>(null);

  const auditQuery = useAuditLogs({
    page,
    pageSize: PAGE_SIZE,
    sort: toSortParam(sort),
  });
  const sessionsQuery = useActiveSessions({ page: sessionPage, pageSize: PAGE_SIZE });
  const terminateMutation = useTerminateSession();

  const auditColumns: DataTableColumn<AuditLogEntry>[] = [
    {
      id: 'occurredAt',
      header: t('admin.security.audit.timestamp'),
      sortable: true,
      cell: (entry) => (
        <span className="whitespace-nowrap text-xs text-charcoal-light">
          {formatDateTime(entry.occurredAt)}
        </span>
      ),
    },
    {
      id: 'actor',
      header: t('admin.security.audit.actor'),
      cell: (entry) => (
        <div className="min-w-0">
          <p className="truncate text-sm text-charcoal">
            {entry.actor.name ?? entry.actor.id ?? t('admin.security.audit.unknownActor')}
          </p>
          <p className="mt-0.5 text-[11px] uppercase tracking-wide text-charcoal-light">
            {t(ACTOR_TYPE_LABEL[entry.actor.type])}
          </p>
        </div>
      ),
    },
    {
      id: 'action',
      header: t('admin.security.audit.action'),
      cell: (entry) => (
        <code className="font-mono text-xs font-semibold text-charcoal">{entry.action}</code>
      ),
    },
    {
      id: 'moduleCode',
      header: t('admin.security.audit.module'),
      cell: (entry) =>
        entry.moduleCode ? (
          <div className="min-w-0">
            <p className="truncate text-xs text-charcoal">{t(getModuleLabel(entry.moduleCode))}</p>
            <code className="mt-0.5 block font-mono text-[11px] text-charcoal-light">
              {entry.moduleCode}
            </code>
          </div>
        ) : (
          <span className="text-charcoal-light">{t('admin.security.audit.emptyValue')}</span>
        ),
    },
    {
      id: 'ipAddress',
      header: t('admin.security.audit.ip'),
      cell: (entry) => (
        <code className="whitespace-nowrap font-mono text-xs text-charcoal">
          {entry.ipAddress ?? t('admin.security.audit.emptyValue')}
        </code>
      ),
    },
    {
      id: 'integrity',
      header: t('admin.security.audit.integrity'),
      cell: (entry) => (
        <span
          className="inline-flex max-w-[9rem] items-center gap-1 text-xs text-charcoal"
          aria-label={t('admin.security.audit.rowHash', { hash: entry.rowHash })}
          title={entry.rowHash}
        >
          <ShieldCheck className="h-3.5 w-3.5 shrink-0 text-teal-hover" aria-hidden="true" />
          <code className="truncate font-mono text-[11px]">{entry.rowHash}</code>
        </span>
      ),
    },
    {
      id: 'diff',
      header: t('admin.security.audit.diff'),
      cell: (entry) => <PayloadDiff before={entry.beforeData} after={entry.afterData} />,
    },
  ];

  const sessionColumns: DataTableColumn<ActiveUserSession>[] = [
    {
      id: 'user',
      header: t('admin.security.sessions.user'),
      cell: (session) => (
        <div className="min-w-0">
          <p className="truncate text-sm font-medium text-charcoal">
            {sessionUserLabel(session, t('admin.security.sessions.unknownUser'))}
          </p>
          <p className="mt-0.5 truncate text-xs text-charcoal-light">{session.email}</p>
        </div>
      ),
    },
    {
      id: 'roles',
      header: t('admin.security.sessions.roles'),
      cell: (session) => (
        <span className="text-xs text-charcoal">
          {session.roles.length > 0
            ? session.roles.join(', ')
            : t('admin.security.audit.emptyValue')}
        </span>
      ),
    },
    {
      id: 'ipAddress',
      header: t('admin.security.sessions.ip'),
      cell: (session) => (
        <code className="whitespace-nowrap font-mono text-xs text-charcoal">
          {session.ipAddress}
        </code>
      ),
    },
    {
      id: 'device',
      header: t('admin.security.sessions.device'),
      cell: (session) => (
        <span
          className="block max-w-[14rem] truncate text-xs text-charcoal-light"
          title={session.userAgent ?? undefined}
        >
          {session.userAgent ?? t('admin.security.sessions.unknownDevice')}
        </span>
      ),
    },
    {
      id: 'lastSeenAt',
      header: t('admin.security.sessions.lastSeenAt'),
      cell: (session) => (
        <span className="whitespace-nowrap text-xs text-charcoal-light">
          {formatDateTime(session.lastSeenAt)}
        </span>
      ),
    },
    {
      id: 'status',
      header: t('admin.security.sessions.status'),
      cell: (session) => (
        <StatusBadge
          label={t(SESSION_STATUS_LABEL[session.status])}
          tone={SESSION_STATUS_TONE[session.status]}
        />
      ),
    },
    {
      id: 'actions',
      header: t('admin.security.sessions.actions'),
      align: 'right',
      cell: (session) => (
        <Button
          size="sm"
          variant="danger"
          aria-label={t('admin.security.sessions.terminateLabel', {
            user: sessionUserLabel(session, t('admin.security.sessions.unknownUser')),
          })}
          onClick={() => {
            terminateMutation.reset();
            setSessionToTerminate(session);
          }}
        >
          {t('admin.security.sessions.terminate')}
        </Button>
      ),
    },
  ];

  const handleTerminate = () => {
    if (!sessionToTerminate) {
      return;
    }

    const userLabel = sessionUserLabel(
      sessionToTerminate,
      t('admin.security.sessions.unknownUser'),
    );

    terminateMutation.mutate(sessionToTerminate.id, {
      onSuccess: () => {
        setAnnouncement(t('admin.security.sessions.terminateSuccess', { user: userLabel }));
        setSessionToTerminate(null);
      },
    });
  };

  const handleCancelTerminate = () => {
    if (terminateMutation.isPending) {
      return;
    }

    setSessionToTerminate(null);
    terminateMutation.reset();
  };

  return (
    <div className="space-y-8">
      <PageHeader
        title={t('nav.admin.security')}
        description={t('page.security.description')}
        status={<StatusBadge label={t('admin.security.audit.chainIntegrity')} tone="success" />}
      />

      {announcement ? (
        <div
          role="status"
          aria-live="polite"
          className="flex items-start gap-2 rounded-lg border border-mint-accent bg-mint-accent/30 px-4 py-3 text-sm text-charcoal"
        >
          <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0" aria-hidden="true" />
          <p>{announcement}</p>
        </div>
      ) : null}

      <section aria-labelledby="audit-heading" className="space-y-4">
        <div>
          <h2 id="audit-heading" className="text-lg font-semibold text-charcoal">
            {t('admin.security.audit.title')}
          </h2>
          <p className="mt-1 text-sm text-charcoal-light">
            {t('admin.security.audit.description')}
          </p>
        </div>

        <QuerySection
          query={auditQuery}
          skeleton={<TableSkeleton columns={auditColumns.length} rows={8} />}
          isEmpty={(data) => data.items.length === 0}
          emptyTitle={t('admin.security.audit.empty.title')}
          emptyDescription={t('admin.security.audit.empty.body')}
          emptyActionLabel={t('admin.security.audit.empty.cta')}
        >
          {(data) => (
            <DataTable
              data={data.items}
              columns={auditColumns}
              getRowId={(entry) => entry.id}
              caption={t('admin.security.audit.tableCaption')}
              sort={sort}
              onSortChange={(next) => {
                setSort(next);
                setPage(1);
              }}
              pagination={{
                page: data.page,
                pageSize: data.pageSize,
                totalItems: data.totalItems,
                totalPages: data.totalPages,
              }}
              onPageChange={setPage}
            />
          )}
        </QuerySection>
      </section>

      <section aria-labelledby="sessions-heading" className="space-y-4">
        <div>
          <h2 id="sessions-heading" className="text-lg font-semibold text-charcoal">
            {t('admin.security.sessions.title')}
          </h2>
          <p className="mt-1 text-sm text-charcoal-light">
            {t('admin.security.sessions.description')}
          </p>
        </div>

        <QuerySection
          query={sessionsQuery}
          skeleton={<TableSkeleton columns={sessionColumns.length} rows={5} />}
          isEmpty={(data) => data.items.length === 0}
          emptyTitle={t('admin.security.sessions.empty.title')}
          emptyDescription={t('admin.security.sessions.empty.body')}
          emptyActionLabel={t('admin.security.sessions.empty.cta')}
        >
          {(data) => (
            <DataTable
              data={data.items}
              columns={sessionColumns}
              getRowId={(session) => session.id}
              caption={t('admin.security.sessions.tableCaption')}
              pagination={{
                page: data.page,
                pageSize: data.pageSize,
                totalItems: data.totalItems,
                totalPages: data.totalPages,
              }}
              onPageChange={setSessionPage}
            />
          )}
        </QuerySection>
      </section>

      <ConfirmDialog
        isOpen={sessionToTerminate !== null}
        title={
          sessionToTerminate
            ? t('admin.security.sessions.terminateTitle', {
                user: sessionUserLabel(
                  sessionToTerminate,
                  t('admin.security.sessions.unknownUser'),
                ),
              })
            : ''
        }
        description={t('admin.security.sessions.terminateBody')}
        confirmLabel={t('admin.security.sessions.terminateConfirm')}
        pendingLabel={t('admin.security.sessions.terminatePending')}
        cancelLabel={t('admin.security.sessions.cancel')}
        isPending={terminateMutation.isPending}
        error={terminateMutation.error}
        errorDescription={t('admin.security.sessions.terminateError')}
        onConfirm={handleTerminate}
        onCancel={handleCancelTerminate}
      />
    </div>
  );
}
