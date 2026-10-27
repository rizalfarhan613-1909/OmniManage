'use client';

import { useI18n } from '@/components/providers/LanguageProvider';
import { StatusBadge } from '@/components/shared/StatusBadge';
import { MODULE_CODES } from '@/config/module-codes';
import { getModuleLabel } from '@/config/module-labels';
import type { DashboardRecordCounts } from '@/features/dashboard/api/types';
import { formatCount } from '@/lib/utils/format';

type Props = {
  modules: readonly string[];
  counts: DashboardRecordCounts;
};

function recordCountFor(moduleCode: string, counts: DashboardRecordCounts): number | null {
  switch (moduleCode) {
    case MODULE_CODES.salesCore:
      return counts.salesOrders;
    case MODULE_CODES.coreMasterData:
      return counts.items;
    case MODULE_CODES.invCore:
      return counts.stockBalances;
    case MODULE_CODES.finAr:
      return counts.arInvoices;
    case MODULE_CODES.finGl:
      return counts.journalEntries;
    default:
      return null;
  }
}

/** Licensed-module summary; modules without a read API show no fabricated usage. */
export function ModuleUtilization({ modules, counts }: Props) {
  const { t } = useI18n();

  if (modules.length === 0) {
    return (
      <p className="py-8 text-center text-sm text-charcoal-light">
        {t('dashboard.modules.noRecords')}
      </p>
    );
  }

  const maxCount = Math.max(
    1,
    ...modules.map((moduleCode) => recordCountFor(moduleCode, counts) ?? 0),
  );

  return (
    <ul className="space-y-2">
      {modules.map((moduleCode) => {
        const count = recordCountFor(moduleCode, counts);
        const label = getModuleLabel(moduleCode);

        return (
          <li key={moduleCode} className="rounded-md border border-line bg-surface px-3 py-2.5">
            <div className="flex items-start justify-between gap-3">
              <div className="min-w-0">
                <p className="truncate text-sm font-medium text-charcoal">{t(label)}</p>
                <p className="truncate font-mono text-xs text-charcoal-light">{moduleCode}</p>
              </div>
              <StatusBadge label={t('dashboard.modules.licensed')} tone="success" />
            </div>
            <div className="mt-2">
              {count === null ? (
                <p className="text-xs text-charcoal-light">{t('dashboard.modules.noRecords')}</p>
              ) : (
                <>
                  <p className="text-xs tabular-nums text-charcoal-light">
                    {t('dashboard.modules.records', { count: formatCount(count) })}
                  </p>
                  <progress
                    value={count}
                    max={maxCount}
                    aria-label={t('dashboard.modules.records', {
                      count: formatCount(count),
                    })}
                    className="mt-1 h-1 w-full overflow-hidden rounded-full accent-teal-primary"
                  />
                </>
              )}
            </div>
          </li>
        );
      })}
    </ul>
  );
}
