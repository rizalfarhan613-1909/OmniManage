'use client';

import { CheckCircle2, Layers } from 'lucide-react';
import { useMemo, useState } from 'react';

import { useI18n } from '@/components/providers/LanguageProvider';
import { ApiErrorState } from '@/components/shared/ApiErrorState';
import { ConfirmDialog } from '@/components/shared/ConfirmDialog';
import { DataTable, type DataTableColumn } from '@/components/shared/DataTable';
import { EmptyState } from '@/components/shared/EmptyState';
import { PageHeader } from '@/components/shared/PageHeader';
import { QuerySection } from '@/components/shared/QuerySection';
import { TableSkeleton } from '@/components/shared/Skeletons';
import { StatusBadge, type StatusTone } from '@/components/shared/StatusBadge';
import { Button } from '@/components/ui/Button';
import {
  MODULE_GROUP_LABEL_KEYS,
  LICENSABLE_MODULE_GROUPS,
  modulesByGroup,
  type LicensableModule,
} from '@/config/licensable-modules';
import { getModuleLabel } from '@/config/module-labels';
import { useTenants } from '@/features/admin/api/use-tenants';
import { useToggleTenantModule } from '@/features/admin/api/use-toggle-tenant-module';
import type { TenantStatus, TenantSummary } from '@/features/admin/api/types';
import type { TranslationKey } from '@/lib/i18n';
import { cn } from '@/lib/utils/cn';
import { formatCount } from '@/lib/utils/format';

import { ModuleLicenseSwitch } from './ModuleLicenseSwitch';

const PAGE_SIZE = 8;

const STATUS_TONE: Record<TenantStatus, StatusTone> = {
  Active: 'success',
  Suspended: 'warning',
  Archived: 'neutral',
};

const STATUS_LABEL: Record<TenantStatus, TranslationKey> = {
  Active: 'admin.licensing.status.active',
  Suspended: 'admin.licensing.status.suspended',
  Archived: 'admin.licensing.status.archived',
};

type PendingDisable = {
  module: LicensableModule;
  tenant: TenantSummary;
};

export function LicensingView() {
  const { t } = useI18n();
  const [page, setPage] = useState(1);
  const [selectedTenantId, setSelectedTenantId] = useState<string | null>(null);
  const [pendingModuleCode, setPendingModuleCode] = useState<string | null>(null);
  const [pendingDisable, setPendingDisable] = useState<PendingDisable | null>(null);
  const [reason, setReason] = useState('');
  const [hasReasonError, setHasReasonError] = useState(false);
  const [announcement, setAnnouncement] = useState<string | null>(null);

  const tenantsQuery = useTenants({ page, pageSize: PAGE_SIZE, sort: 'name' });
  const toggleMutation = useToggleTenantModule();

  const selectedTenant = useMemo(() => {
    const tenants = tenantsQuery.data?.items ?? [];
    return tenants.find((tenant) => tenant.id === selectedTenantId) ?? tenants[0] ?? null;
  }, [selectedTenantId, tenantsQuery.data]);

  const applyChange = (
    tenant: TenantSummary,
    module: LicensableModule,
    isActive: boolean,
    changeReason: string,
  ) => {
    setPendingModuleCode(module.code);
    toggleMutation.mutate(
      {
        tenantId: tenant.id,
        moduleCode: module.code,
        isActive,
        reason: changeReason.trim(),
      },
      {
        onSuccess: () => {
          setAnnouncement(
            t(isActive ? 'admin.licensing.matrix.enabled' : 'admin.licensing.matrix.disabled', {
              module: t(getModuleLabel(module.code)),
              tenant: tenant.name,
            }),
          );
          setPendingDisable(null);
        },
        onSettled: () => setPendingModuleCode(null),
      },
    );
  };

  const handleToggle = (module: LicensableModule) => {
    if (!selectedTenant) {
      return;
    }

    toggleMutation.reset();
    setAnnouncement(null);

    if (selectedTenant.licensedModules.includes(module.code)) {
      setReason('');
      setHasReasonError(false);
      setPendingDisable({ module, tenant: selectedTenant });
      return;
    }

    applyChange(selectedTenant, module, true, t('admin.licensing.matrix.enableReason'));
  };

  const handleConfirmDisable = () => {
    if (!pendingDisable) {
      return;
    }

    if (reason.trim().length === 0) {
      setHasReasonError(true);
      return;
    }

    applyChange(pendingDisable.tenant, pendingDisable.module, false, reason);
  };

  const handleCancelDisable = () => {
    if (toggleMutation.isPending) {
      return;
    }

    setPendingDisable(null);
    toggleMutation.reset();
  };

  const handleSelectTenant = (tenantId: string) => {
    setSelectedTenantId(tenantId);
    setAnnouncement(null);
    toggleMutation.reset();
  };

  const handlePageChange = (nextPage: number) => {
    setPage(nextPage);
    setSelectedTenantId(null);
    setAnnouncement(null);
  };

  const columns: DataTableColumn<TenantSummary>[] = [
    {
      id: 'tenant',
      header: t('admin.licensing.tenants.tenant'),
      cell: (tenant) => (
        <div className="min-w-0">
          <p className="truncate text-sm font-semibold text-charcoal">{tenant.name}</p>
          <p className="mt-0.5 font-mono text-xs text-charcoal-light">{tenant.code}</p>
        </div>
      ),
    },
    {
      id: 'planCode',
      header: t('admin.licensing.tenants.plan'),
      cell: (tenant) => (
        <span className="inline-flex rounded-full border border-line bg-surface-muted px-2.5 py-0.5 text-xs font-medium text-charcoal">
          {tenant.planCode}
        </span>
      ),
    },
    {
      id: 'status',
      header: t('admin.licensing.tenants.status'),
      cell: (tenant) => (
        <StatusBadge label={t(STATUS_LABEL[tenant.status])} tone={STATUS_TONE[tenant.status]} />
      ),
    },
    {
      id: 'activeUsers',
      header: t('admin.licensing.tenants.users'),
      align: 'right',
      className: 'tabular-nums',
      cell: (tenant) =>
        t('admin.licensing.tenants.usersValue', {
          active: formatCount(tenant.activeUserCount),
          quota: formatCount(tenant.activeUserQuota),
        }),
    },
    {
      id: 'licensedModules',
      header: t('admin.licensing.tenants.licensed'),
      align: 'right',
      className: 'tabular-nums',
      cell: (tenant) => formatCount(tenant.licensedModules.length),
    },
    {
      id: 'actions',
      header: t('admin.licensing.tenants.actions'),
      align: 'right',
      cell: (tenant) => {
        const isSelected = selectedTenant?.id === tenant.id;

        return (
          <Button
            size="sm"
            variant={isSelected ? 'primary' : 'secondary'}
            aria-pressed={isSelected}
            onClick={() => handleSelectTenant(tenant.id)}
          >
            {isSelected
              ? t('admin.licensing.tenants.selected')
              : t('admin.licensing.tenants.manage')}
          </Button>
        );
      },
    },
  ];

  return (
    <div className="space-y-8">
      <PageHeader title={t('nav.admin.licensing')} description={t('page.licensing.description')} />

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

      {toggleMutation.error && !pendingDisable ? (
        <ApiErrorState error={toggleMutation.error} />
      ) : null}

      <section aria-labelledby="tenant-directory-heading" className="space-y-4">
        <div>
          <h2 id="tenant-directory-heading" className="text-lg font-semibold text-charcoal">
            {t('admin.licensing.tenants.title')}
          </h2>
          <p className="mt-1 text-sm text-charcoal-light">
            {t('admin.licensing.tenants.description')}
          </p>
        </div>

        <QuerySection
          query={tenantsQuery}
          skeleton={<TableSkeleton columns={columns.length} rows={6} />}
          isEmpty={(data) => data.items.length === 0}
          emptyTitle={t('admin.licensing.tenants.empty.title')}
          emptyDescription={t('admin.licensing.tenants.empty.body')}
          emptyActionLabel={t('admin.licensing.tenants.empty.cta')}
        >
          {(data) => (
            <div className="space-y-8">
              <DataTable
                data={data.items}
                columns={columns}
                getRowId={(tenant) => tenant.id}
                caption={t('admin.licensing.tenants.tableCaption')}
                pagination={{
                  page: data.page,
                  pageSize: data.pageSize,
                  totalItems: data.totalItems,
                  totalPages: data.totalPages,
                }}
                onPageChange={handlePageChange}
              />

              <section aria-labelledby="license-matrix-heading" className="space-y-4">
                <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <h2 id="license-matrix-heading" className="text-lg font-semibold text-charcoal">
                      {t('admin.licensing.matrix.title')}
                    </h2>
                    {selectedTenant ? (
                      <p className="mt-1 text-sm text-charcoal-light">
                        {t('admin.licensing.matrix.description', {
                          tenant: selectedTenant.name,
                        })}
                      </p>
                    ) : null}
                  </div>
                  {selectedTenant ? (
                    <StatusBadge
                      label={t(STATUS_LABEL[selectedTenant.status])}
                      tone={STATUS_TONE[selectedTenant.status]}
                    />
                  ) : null}
                </div>

                {selectedTenant ? (
                  <div className="space-y-4">
                    {LICENSABLE_MODULE_GROUPS.map((group) => {
                      const modules = modulesByGroup(group);

                      if (modules.length === 0) {
                        return null;
                      }

                      const headingId = `license-group-${group}`;

                      return (
                        <section
                          key={group}
                          aria-labelledby={headingId}
                          className="overflow-hidden rounded-lg border border-line bg-surface shadow-sm"
                        >
                          <h3
                            id={headingId}
                            className="border-b border-line bg-surface-muted/60 px-4 py-3 text-xs font-semibold uppercase tracking-wide text-charcoal-light"
                          >
                            {t(MODULE_GROUP_LABEL_KEYS[group])}
                          </h3>
                          <ul className="divide-y divide-sage/60">
                            {modules.map((module) => {
                              const isActive = selectedTenant.licensedModules.includes(module.code);
                              const dependencyMissing =
                                module.dependsOn !== undefined &&
                                !isActive &&
                                !selectedTenant.licensedModules.includes(module.dependsOn);
                              const isPending = pendingModuleCode === module.code;
                              const dependencyHintId = `license-dependency-${module.code}`;

                              return (
                                <li
                                  key={module.code}
                                  className="flex flex-col gap-3 px-4 py-4 sm:flex-row sm:items-center sm:justify-between"
                                >
                                  <div className="min-w-0">
                                    <div className="flex flex-wrap items-center gap-2">
                                      <p className="text-sm font-medium text-charcoal">
                                        {t(getModuleLabel(module.code))}
                                      </p>
                                      <code className="rounded-sm bg-surface-muted px-1.5 py-0.5 font-mono text-[11px] text-charcoal-light">
                                        {module.code}
                                      </code>
                                    </div>
                                    <p className="mt-1 text-xs text-charcoal-light">
                                      {t(module.descriptionKey)}
                                    </p>
                                    {dependencyMissing && module.dependsOn ? (
                                      <p
                                        id={dependencyHintId}
                                        className="mt-1 text-xs font-medium text-warning"
                                      >
                                        {t('admin.licensing.matrix.requires', {
                                          module: t(getModuleLabel(module.dependsOn)),
                                        })}
                                      </p>
                                    ) : null}
                                  </div>

                                  <ModuleLicenseSwitchRow
                                    module={module}
                                    tenant={selectedTenant}
                                    isActive={isActive}
                                    isPending={isPending}
                                    descriptionId={dependencyMissing ? dependencyHintId : undefined}
                                    isDisabled={
                                      dependencyMissing || selectedTenant.status === 'Archived'
                                    }
                                    onToggle={() => handleToggle(module)}
                                  />
                                </li>
                              );
                            })}
                          </ul>
                        </section>
                      );
                    })}
                  </div>
                ) : (
                  <EmptyState
                    icon={Layers}
                    title={t('admin.licensing.matrix.noTenant.title')}
                    description={t('admin.licensing.matrix.noTenant.body')}
                  />
                )}
              </section>
            </div>
          )}
        </QuerySection>
      </section>

      <ConfirmDialog
        isOpen={pendingDisable !== null}
        title={
          pendingDisable
            ? t('admin.licensing.disable.title', {
                module: t(getModuleLabel(pendingDisable.module.code)),
              })
            : ''
        }
        description={
          pendingDisable
            ? t('admin.licensing.disable.body', {
                tenant: pendingDisable.tenant.name,
                module: t(getModuleLabel(pendingDisable.module.code)),
              })
            : ''
        }
        confirmLabel={t('admin.licensing.disable.confirm')}
        pendingLabel={t('admin.licensing.disable.pending')}
        cancelLabel={t('admin.licensing.cancel')}
        isPending={toggleMutation.isPending}
        isConfirmDisabled={reason.trim().length === 0}
        error={toggleMutation.error}
        errorDescription={t('admin.licensing.matrix.toggleError')}
        onConfirm={handleConfirmDisable}
        onCancel={handleCancelDisable}
      >
        <label htmlFor="license-change-reason" className="block text-sm font-medium text-charcoal">
          {t('admin.licensing.disable.reasonLabel')}
        </label>
        <textarea
          id="license-change-reason"
          rows={3}
          value={reason}
          aria-invalid={hasReasonError || undefined}
          aria-describedby="license-change-reason-hint"
          onChange={(event) => {
            setReason(event.target.value);
            setHasReasonError(false);
          }}
          placeholder={t('admin.licensing.disable.reasonLabel')}
          className={cn(
            'mt-1 w-full rounded-md border bg-surface px-3 py-2 text-base text-charcoal shadow-sm placeholder:text-charcoal-light focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary',
            hasReasonError ? 'border-danger' : 'border-line',
          )}
        />
        <p
          id="license-change-reason-hint"
          className={cn('mt-1 text-xs', hasReasonError ? 'text-danger' : 'text-charcoal-light')}
        >
          {hasReasonError
            ? t('admin.licensing.disable.reasonRequired')
            : t('admin.licensing.disable.reasonHint')}
        </p>
      </ConfirmDialog>
    </div>
  );
}

type SwitchRowProps = {
  module: LicensableModule;
  tenant: TenantSummary;
  isActive: boolean;
  isPending: boolean;
  descriptionId?: string;
  isDisabled: boolean;
  onToggle: () => void;
};

function ModuleLicenseSwitchRow({
  module,
  tenant,
  isActive,
  isPending,
  descriptionId,
  isDisabled,
  onToggle,
}: SwitchRowProps) {
  const { t } = useI18n();
  const label = t('admin.licensing.matrix.toggleLabel', {
    module: t(getModuleLabel(module.code)),
    tenant: tenant.name,
  });

  return (
    <ModuleLicenseSwitch
      checked={isActive}
      label={label}
      descriptionId={descriptionId}
      isPending={isPending}
      disabled={isDisabled}
      onToggle={onToggle}
    />
  );
}
