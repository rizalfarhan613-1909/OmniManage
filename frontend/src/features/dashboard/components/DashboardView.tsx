'use client';

import { Banknote, Boxes, Landmark, ReceiptText, TrendingUp } from 'lucide-react';

import { ApiErrorState } from '@/components/shared/ApiErrorState';
import { ChartCard } from '@/components/shared/ChartCard';
import { EmptyState } from '@/components/shared/EmptyState';
import { KpiCard } from '@/components/shared/KpiCard';
import { PageHeader } from '@/components/shared/PageHeader';
import { DashboardSkeleton } from '@/components/shared/Skeletons';
import { StatusBadge } from '@/components/shared/StatusBadge';
import { Button } from '@/components/ui/Button';
import { useSession } from '@/components/providers/SessionProvider';
import { useExecutiveDashboard } from '@/features/dashboard/api/use-executive-dashboard';
import { t } from '@/lib/i18n';
import { toDecimal } from '@/lib/utils/decimal';
import { formatCount, formatMoney } from '@/lib/utils/format';

import { ActivityFeed } from './ActivityFeed';
import { ModuleUtilization } from './ModuleUtilization';
import { RevenueChart } from './RevenueChart';

export function DashboardView() {
  const session = useSession();
  const { data, isLoading, error, refetch, isEnabled } = useExecutiveDashboard();

  const header = (
    <PageHeader
      title={t('nav.dashboard')}
      description={t('page.dashboard.description')}
      actions={<StatusBadge label={t('dashboard.live')} tone="info" />}
    />
  );

  if (!isEnabled) {
    return (
      <div className="space-y-6">
        {header}
        <EmptyState
          title={t('dashboard.empty.title')}
          description={t('dashboard.empty.body')}
          action={<Button onClick={refetch}>{t('dashboard.empty.cta')}</Button>}
        />
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        {header}
        <DashboardSkeleton />
      </div>
    );
  }

  if (error || !data) {
    return (
      <div className="space-y-6">
        {header}
        <ApiErrorState error={error} onRetry={refetch} />
      </div>
    );
  }

  const totalRecords =
    data.counts.salesOrders +
    data.counts.arInvoices +
    data.counts.journalEntries +
    data.counts.stockBalances +
    data.counts.items;

  if (totalRecords === 0) {
    return (
      <div className="space-y-6">
        {header}
        <EmptyState
          title={t('dashboard.empty.title')}
          description={t('dashboard.empty.body')}
          action={<Button onClick={refetch}>{t('dashboard.empty.cta')}</Button>}
        />
      </div>
    );
  }

  const change = data.revenueChangePercent ? toDecimal(data.revenueChangePercent) : null;
  const trendDirection = change
    ? change.greaterThan(0)
      ? 'up'
      : change.lessThan(0)
        ? 'down'
        : 'flat'
    : 'flat';
  const trendLabel = change
    ? t('dashboard.kpi.trend', { value: change.toFixed(1) })
    : t('dashboard.kpi.trendFlat');

  const showRevenue = data.available.sales;
  const showCashFlow = data.available.financeAr;
  const hasSeries = data.revenueSeries.some(
    (point) =>
      (showRevenue && point.revenueValue !== 0) || (showCashFlow && point.cashFlowValue !== 0),
  );

  return (
    <div className="space-y-6">
      {header}

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {data.available.sales ? (
          <KpiCard
            label={t('dashboard.kpi.revenue')}
            value={formatMoney(data.revenueMtd)}
            helper={t('dashboard.kpi.revenueHelper')}
            icon={TrendingUp}
            href="/sales"
            trend={{ direction: trendDirection, label: trendLabel }}
          />
        ) : null}
        {data.available.financeAr ? (
          <KpiCard
            label={t('dashboard.kpi.openAr')}
            value={formatMoney(data.openAr)}
            helper={t('dashboard.kpi.openArHelper')}
            icon={ReceiptText}
            href="/finance"
          />
        ) : null}
        {data.available.stockValue ? (
          <KpiCard
            label={t('dashboard.kpi.stockValue')}
            value={formatMoney(data.stockValue)}
            helper={t('dashboard.kpi.stockValueHelper')}
            icon={Boxes}
            href="/inventory"
          />
        ) : null}
        {data.available.financeAr ? (
          <KpiCard
            label={t('dashboard.kpi.cashCollected')}
            value={formatMoney(data.cashCollected)}
            helper={t('dashboard.kpi.cashCollectedHelper')}
            icon={Banknote}
            href="/finance"
          />
        ) : null}
      </div>

      <div className="grid gap-6 xl:grid-cols-3">
        <div className="xl:col-span-2">
          <ChartCard
            title={t('dashboard.chart.title')}
            description={t('dashboard.chart.description')}
            action={<StatusBadge label={t('dashboard.live')} tone="info" />}
          >
            {hasSeries ? (
              <RevenueChart
                data={data.revenueSeries}
                showRevenue={showRevenue}
                showCashFlow={showCashFlow}
              />
            ) : (
              <p className="py-16 text-center text-sm text-charcoal-light">
                {t('dashboard.chart.empty')}
              </p>
            )}
          </ChartCard>
        </div>

        <ChartCard
          title={t('dashboard.activity.title')}
          description={t('dashboard.activity.description')}
          action={<Landmark className="h-4 w-4 text-charcoal-light" aria-hidden="true" />}
        >
          <ActivityFeed items={data.activity} />
        </ChartCard>
      </div>

      <ChartCard
        title={t('dashboard.modules.title')}
        description={t('dashboard.modules.description')}
        action={
          <span className="text-xs tabular-nums text-charcoal-light">
            {t('common.records', { count: formatCount(totalRecords) })}
          </span>
        }
      >
        <ModuleUtilization modules={session.activeModules} counts={data.counts} />
      </ChartCard>
    </div>
  );
}
