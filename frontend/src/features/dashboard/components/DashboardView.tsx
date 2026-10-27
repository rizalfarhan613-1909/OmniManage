'use client';

import {
  Activity,
  ArrowUpRight,
  Banknote,
  Boxes,
  CalendarDays,
  Download,
  Gauge,
  Layers,
  ReceiptText,
  ShieldCheck,
  TrendingUp,
  type LucideIcon,
} from 'lucide-react';

import { useI18n } from '@/components/providers/LanguageProvider';
import { useSession } from '@/components/providers/SessionProvider';
import { ApiErrorState } from '@/components/shared/ApiErrorState';
import { ChartCard } from '@/components/shared/ChartCard';
import { EmptyState } from '@/components/shared/EmptyState';
import { DashboardSkeleton } from '@/components/shared/Skeletons';
import { StatusBadge } from '@/components/shared/StatusBadge';
import { Button } from '@/components/ui/Button';
import { CHART_COLORS } from '@/config/chart-colors';
import type { ActivityKind } from '@/features/dashboard/api/types';
import { useExecutiveDashboard } from '@/features/dashboard/api/use-executive-dashboard';
import type { TranslationKey } from '@/lib/i18n';
import { toDecimal } from '@/lib/utils/decimal';
import { formatCount, formatMoney, formatRelativeTime } from '@/lib/utils/format';
import { formatPersonName } from '@/lib/utils/names';
import { cn } from '@/lib/utils/cn';

import { ModuleUtilization } from './ModuleUtilization';
import { RevenueChart } from './RevenueChart';

const ACTIVITY_LABELS: Record<ActivityKind, TranslationKey> = {
  salesConfirmed: 'dashboard.activity.salesConfirmed',
  salesDraft: 'dashboard.activity.salesDraft',
  invoicePaid: 'dashboard.activity.invoicePaid',
  invoiceUnpaid: 'dashboard.activity.invoiceUnpaid',
  journalPosted: 'dashboard.activity.journalPosted',
};

const ACTIVITY_DOMAIN: Record<ActivityKind, TranslationKey> = {
  salesConfirmed: 'dashboard.domain.sales',
  salesDraft: 'dashboard.domain.sales',
  invoicePaid: 'dashboard.domain.finance',
  invoiceUnpaid: 'dashboard.domain.finance',
  journalPosted: 'dashboard.domain.ledger',
};

const ACTIVITY_IMPACT: Record<ActivityKind, TranslationKey> = {
  salesConfirmed: 'dashboard.impact.salesConfirmed',
  salesDraft: 'dashboard.impact.salesDraft',
  invoicePaid: 'dashboard.impact.invoicePaid',
  invoiceUnpaid: 'dashboard.impact.invoiceUnpaid',
  journalPosted: 'dashboard.impact.journalPosted',
};

const ACTIVITY_STATUS: Record<ActivityKind, { labelKey: TranslationKey; className: string }> = {
  salesConfirmed: {
    labelKey: 'dashboard.status.nominal',
    className: 'bg-mint-accent/30 text-on-accent dark:bg-mint-accent/15 dark:text-mint-accent',
  },
  salesDraft: {
    labelKey: 'dashboard.status.pending',
    className: 'bg-amber-100 text-amber-700 dark:bg-amber-500/15 dark:text-amber-300',
  },
  invoicePaid: {
    labelKey: 'dashboard.status.optimized',
    className: 'bg-cyan/15 text-cyan-deep',
  },
  invoiceUnpaid: {
    labelKey: 'dashboard.status.monitor',
    className: 'bg-amber-100 text-amber-700 dark:bg-amber-500/15 dark:text-amber-300',
  },
  journalPosted: {
    labelKey: 'dashboard.status.nominal',
    className: 'bg-mint-accent/30 text-on-accent dark:bg-mint-accent/15 dark:text-mint-accent',
  },
};

type SparklineProps = {
  values: readonly number[];
};

function Sparkline({ values }: SparklineProps) {
  if (values.length < 2) {
    return <div className="h-10 w-full" aria-hidden="true" />;
  }

  const maximum = Math.max(...values);
  const minimum = Math.min(...values);
  const range = maximum - minimum || 1;
  const points = values
    .map((value, index) => {
      const x = (index / (values.length - 1)) * 100;
      const y = 38 - ((value - minimum) / range) * 34;

      return `${x.toFixed(2)},${y.toFixed(2)}`;
    })
    .join(' ');

  return (
    <svg viewBox="0 0 100 40" preserveAspectRatio="none" className="h-10 w-full" aria-hidden="true">
      <polyline
        points={points}
        fill="none"
        stroke={CHART_COLORS.revenue}
        strokeWidth="2.4"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}

type MetricCardProps = {
  label: string;
  value: string;
  helper: string;
  icon: LucideIcon;
  trend?: { label: string; positive?: boolean };
  sparkline?: readonly number[];
  progress?: number;
  accent?: 'cyan' | 'mint';
};

function MetricCard({
  label,
  value,
  helper,
  icon: Icon,
  trend,
  sparkline,
  progress,
  accent = 'cyan',
}: MetricCardProps) {
  return (
    <article className="relative flex flex-col overflow-hidden rounded-2xl border border-line bg-surface p-4 shadow-sm transition-all duration-300 hover:-translate-y-0.5 hover:shadow-md">
      <div className="flex items-start justify-between gap-3">
        <p className="text-[10px] font-bold uppercase tracking-[0.14em] text-charcoal-light">
          {label}
        </p>
        <span
          className={cn(
            'flex h-8 w-8 shrink-0 items-center justify-center rounded-lg',
            accent === 'mint'
              ? 'bg-mint-accent/25 text-on-accent dark:bg-mint-accent/15 dark:text-mint-accent'
              : 'bg-cyan/10 text-cyan-deep',
          )}
        >
          <Icon className="h-4 w-4" aria-hidden="true" />
        </span>
      </div>

      <p className="mt-2 text-2xl font-bold tabular-nums tracking-[-0.045em] text-charcoal-strong">
        {value}
      </p>

      {trend ? (
        <span
          className={cn(
            'mt-1.5 inline-flex w-fit items-center gap-1 rounded-full px-2 py-0.5 text-[10px] font-bold',
            trend.positive === false
              ? 'bg-amber-100 text-amber-700 dark:bg-amber-500/15 dark:text-amber-300'
              : 'bg-mint-accent/30 text-on-accent dark:bg-mint-accent/15 dark:text-mint-accent',
          )}
        >
          <ArrowUpRight className="h-3 w-3" aria-hidden="true" />
          {trend.label}
        </span>
      ) : null}

      {sparkline ? (
        <div className="mt-2">
          <Sparkline values={sparkline} />
        </div>
      ) : null}

      {progress !== undefined ? (
        <div className="mt-3 h-1.5 w-full overflow-hidden rounded-full bg-surface-muted">
          <div
            className="h-full rounded-full bg-gradient-to-r from-cyan-deep to-cyan"
            style={{ width: `${Math.min(100, Math.max(0, progress))}%` }}
          />
        </div>
      ) : null}

      <p className="mt-auto pt-2 text-[10px] leading-4 text-charcoal-light">{helper}</p>
    </article>
  );
}

export function DashboardView() {
  const session = useSession();
  const { t } = useI18n();
  const { data, isLoading, error, refetch, isEnabled } = useExecutiveDashboard();

  const header = (
    <section className="rounded-2xl border border-line bg-surface p-5 shadow-sm sm:p-6">
      <div className="flex flex-col gap-5 xl:flex-row xl:items-start xl:justify-between">
        <div className="min-w-0">
          <p className="inline-flex items-center gap-2 rounded-full border border-line bg-surface-muted px-2.5 py-1 text-[10px] font-bold uppercase tracking-[0.14em] text-charcoal-light">
            <span className="relative flex h-1.5 w-1.5">
              <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-emerald-400 opacity-60" />
              <span className="relative inline-flex h-1.5 w-1.5 rounded-full bg-emerald-500" />
            </span>
            {t('dashboard.header.live')}
            <span className="text-charcoal-light/50">|</span>
            <span className="font-mono text-charcoal-strong">{session.tenant.code}-EXEC-NODE</span>
          </p>
          <h1 className="mt-3 text-balance text-2xl font-bold tracking-[-0.04em] text-charcoal-strong sm:text-3xl">
            {t('dashboard.header.title')}
          </h1>
          <p className="mt-2 max-w-3xl text-sm text-charcoal-light">
            {t('dashboard.header.description', { tenant: session.tenant.name })}
          </p>
        </div>

        <div className="flex shrink-0 flex-col items-start gap-2 sm:items-end">
          <div className="flex flex-wrap rounded-xl border border-line bg-surface-muted p-1 text-[11px] font-semibold">
            <span className="rounded-lg bg-surface px-3 py-1.5 text-charcoal-strong shadow-sm">
              {t('dashboard.period.q3')}
            </span>
            <span className="px-3 py-1.5 text-charcoal-light">{t('dashboard.period.30d')}</span>
            <span className="px-3 py-1.5 text-charcoal-light">{t('dashboard.period.fy')}</span>
          </div>
          <div className="flex flex-wrap gap-2">
            <Button variant="secondary" size="sm">
              <Download className="h-3.5 w-3.5" aria-hidden="true" />
              {t('dashboard.export')}
            </Button>
            <Button size="sm">
              <CalendarDays className="h-3.5 w-3.5" aria-hidden="true" />
              {t('dashboard.schedule')}
            </Button>
          </div>
        </div>
      </div>
    </section>
  );

  if (!isEnabled) {
    return (
      <div className="space-y-5">
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
      <div className="space-y-5">
        {header}
        <DashboardSkeleton />
      </div>
    );
  }

  if (error || !data) {
    return (
      <div className="space-y-5">
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
      <div className="space-y-5">
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

  const revenueSparkline = data.revenueSeries.map((point) => point.revenueValue);
  const cashSparkline = data.revenueSeries.map((point) => point.cashFlowValue);
  const assignedLead = formatPersonName(session.user.email);
  const capacityPercent = Math.min(
    100,
    Math.round(
      (data.counts.stockBalances / Math.max(1, data.counts.stockBalances + data.counts.items)) *
        100,
    ) + 62,
  );

  return (
    <div className="space-y-5">
      {header}

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {data.available.sales ? (
          <MetricCard
            label={t('dashboard.kpi.revenue')}
            value={formatMoney(data.revenueMtd)}
            helper={t('dashboard.kpi.revenueHelper')}
            icon={TrendingUp}
            trend={{ label: trendLabel, positive: trendDirection !== 'down' }}
            sparkline={revenueSparkline}
            accent="cyan"
          />
        ) : null}
        {data.available.financeAr ? (
          <MetricCard
            label={t('dashboard.kpi.openAr')}
            value={formatMoney(data.openAr)}
            helper={t('dashboard.kpi.openArHelper')}
            icon={ReceiptText}
            progress={Math.min(100, 100 - capacityPercent / 3)}
            accent="mint"
          />
        ) : null}
        {data.available.stockValue ? (
          <MetricCard
            label={t('dashboard.kpi.stockValue')}
            value={formatMoney(data.stockValue)}
            helper={t('dashboard.kpi.stockValueHelper')}
            icon={Boxes}
            progress={capacityPercent}
            accent="cyan"
          />
        ) : null}
        {data.available.financeAr ? (
          <MetricCard
            label={t('dashboard.kpi.cashCollected')}
            value={formatMoney(data.cashCollected)}
            helper={t('dashboard.kpi.cashCollectedHelper')}
            icon={Banknote}
            sparkline={cashSparkline}
            accent="mint"
          />
        ) : null}
      </div>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1.9fr)_minmax(0,1fr)]">
        <ChartCard
          title={t('dashboard.chart.title')}
          description={t('dashboard.chart.description')}
          action={<StatusBadge label={t('dashboard.live')} tone="info" />}
          footer={
            <div className="flex flex-wrap items-center justify-between gap-3 text-[10px] text-charcoal-light">
              <span className="inline-flex items-center gap-4">
                <span className="inline-flex items-center gap-1.5">
                  <span className="h-2 w-2 rounded-full bg-teal-primary" aria-hidden="true" />
                  {t('dashboard.chart.legend.historical')}
                </span>
                <span className="inline-flex items-center gap-1.5">
                  <span className="h-2 w-2 rounded-full bg-mint-accent" aria-hidden="true" />
                  {t('dashboard.chart.legend.cash')}
                </span>
              </span>
              <span className="inline-flex items-center gap-1.5 font-medium">
                <ShieldCheck className="h-3.5 w-3.5 text-emerald-500" aria-hidden="true" />
                {t('dashboard.chart.gaap')}
              </span>
            </div>
          }
        >
          <div className="mb-4 flex flex-wrap gap-2">
            <span className="rounded-lg bg-sidebar px-3 py-1.5 text-[10px] font-bold uppercase tracking-[0.1em] text-sidebar-fg">
              {t('dashboard.chart.consolidated')}
            </span>
            <span className="rounded-lg border border-line bg-surface px-3 py-1.5 text-[10px] font-semibold text-charcoal-light">
              {t('dashboard.chart.opex')}
            </span>
            <span className="rounded-lg border border-line bg-surface px-3 py-1.5 text-[10px] font-semibold text-charcoal-light">
              {t('dashboard.chart.ebitda')}
            </span>
          </div>
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

        <ChartCard
          title={t('dashboard.modules.title')}
          description={t('dashboard.modules.description')}
          action={
            <span className="inline-flex items-center gap-1.5 rounded-full bg-cyan/10 px-2.5 py-1 text-[10px] font-bold text-cyan-deep">
              <Layers className="h-3.5 w-3.5" aria-hidden="true" />
              {t('dashboard.modules.active', { count: session.activeModules.length })}
            </span>
          }
          footer={
            <div className="flex items-center justify-between gap-3 rounded-xl bg-surface-muted p-3">
              <div>
                <p className="text-[9px] font-bold uppercase tracking-[0.14em] text-charcoal-light">
                  {t('dashboard.modules.spend')}
                </p>
                <p className="mt-0.5 text-lg font-bold tracking-tight text-charcoal-strong">
                  {t('dashboard.modules.spendValue')}
                  <span className="text-xs font-medium text-charcoal-light">
                    {t('dashboard.modules.perMonth')}
                  </span>
                </p>
              </div>
              <Button variant="secondary" size="sm">
                {t('dashboard.modules.manage')}
              </Button>
            </div>
          }
        >
          <ModuleUtilization modules={session.activeModules} counts={data.counts} />
        </ChartCard>
      </div>

      <section
        aria-label={t('dashboard.pulse.aria')}
        className="rounded-2xl border border-line bg-surface shadow-sm"
      >
        <div className="flex flex-wrap items-center justify-between gap-3 border-b border-line px-5 py-4">
          <div className="flex items-center gap-3">
            <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-cyan/10 text-cyan-deep">
              <Activity className="h-5 w-5" aria-hidden="true" />
            </span>
            <div>
              <h2 className="text-base font-bold tracking-tight text-charcoal-strong">
                {t('dashboard.pulse.title')}
              </h2>
              <p className="text-[11px] text-charcoal-light">{t('dashboard.pulse.description')}</p>
            </div>
          </div>
          <div className="flex flex-wrap items-center gap-2">
            <span className="inline-flex items-center gap-1.5 rounded-full bg-mint-accent/30 px-2.5 py-1 text-[10px] font-bold text-on-accent dark:bg-mint-accent/15 dark:text-mint-accent">
              <ShieldCheck className="h-3.5 w-3.5" aria-hidden="true" />
              {t('dashboard.pulse.faults')}
            </span>
            <Button variant="secondary" size="sm">
              <Gauge className="h-3.5 w-3.5" aria-hidden="true" />
              {t('dashboard.pulse.auditStream')}
            </Button>
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full min-w-[880px] text-left text-xs">
            <caption className="sr-only">{t('dashboard.pulse.caption')}</caption>
            <thead>
              <tr className="border-b border-line bg-surface-muted text-[9px] font-bold uppercase tracking-[0.12em] text-charcoal-light">
                <th scope="col" className="px-5 py-3">
                  {t('dashboard.pulse.columns.signal')}
                </th>
                <th scope="col" className="px-4 py-3">
                  {t('dashboard.pulse.columns.domain')}
                </th>
                <th scope="col" className="px-4 py-3">
                  {t('dashboard.pulse.columns.timestamp')}
                </th>
                <th scope="col" className="px-4 py-3">
                  {t('dashboard.pulse.columns.impact')}
                </th>
                <th scope="col" className="px-4 py-3">
                  {t('dashboard.pulse.columns.lead')}
                </th>
                <th scope="col" className="px-5 py-3">
                  {t('dashboard.pulse.columns.status')}
                </th>
              </tr>
            </thead>
            <tbody>
              {data.activity.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-5 py-8 text-center text-sm text-charcoal-light">
                    {t('dashboard.activity.empty')}
                  </td>
                </tr>
              ) : (
                data.activity.map((item) => {
                  const status = ACTIVITY_STATUS[item.kind];

                  return (
                    <tr
                      key={item.id}
                      className="border-b border-line/70 last:border-0 hover:bg-surface-muted/70"
                    >
                      <td className="px-5 py-3">
                        <span className="flex items-center gap-2">
                          <span
                            className="h-2 w-2 shrink-0 rounded-full bg-emerald-500"
                            aria-hidden="true"
                          />
                          <span className="font-medium text-charcoal-strong">
                            {t(ACTIVITY_LABELS[item.kind], { reference: item.reference })}
                          </span>
                        </span>
                      </td>
                      <td className="px-4 py-3 text-charcoal-light">
                        {t(ACTIVITY_DOMAIN[item.kind])}
                      </td>
                      <td className="px-4 py-3">
                        <time
                          dateTime={item.occurredAt}
                          className="font-mono text-[10px] text-charcoal-light"
                        >
                          {formatRelativeTime(item.occurredAt)}
                        </time>
                      </td>
                      <td className="px-4 py-3">
                        <span className="block font-mono text-[11px] font-semibold text-charcoal-strong">
                          {formatMoney(item.amount)}
                        </span>
                        <span className="block text-[10px] text-charcoal-light">
                          {t(ACTIVITY_IMPACT[item.kind])}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-charcoal-light">{assignedLead}</td>
                      <td className="px-5 py-3">
                        <span
                          className={cn(
                            'inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-[10px] font-bold',
                            status.className,
                          )}
                        >
                          <span
                            className="h-1.5 w-1.5 rounded-full bg-current"
                            aria-hidden="true"
                          />
                          {t(status.labelKey)}
                        </span>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>

        <div className="flex flex-wrap items-center justify-between gap-2 border-t border-line bg-surface-muted px-5 py-3 text-[10px] text-charcoal-light">
          <span>
            {t('dashboard.pulse.showing', {
              shown: Math.min(data.activity.length, 8),
              total: formatCount(totalRecords),
            })}
          </span>
          <span className="inline-flex items-center gap-1.5 font-mono">
            <ShieldCheck className="h-3 w-3 text-emerald-500" aria-hidden="true" />
            {t('dashboard.pulse.verified')}
          </span>
        </div>
      </section>
    </div>
  );
}
