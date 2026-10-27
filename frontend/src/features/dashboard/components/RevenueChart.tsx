'use client';

import { useId } from 'react';
import {
  Area,
  AreaChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';

import { useI18n } from '@/components/providers/LanguageProvider';
import { CHART_COLORS } from '@/config/chart-colors';
import type { RevenuePoint } from '@/features/dashboard/api/types';
import { formatCompactMoney, formatMoney } from '@/lib/utils/format';

type Props = {
  data: readonly RevenuePoint[];
  showRevenue: boolean;
  showCashFlow: boolean;
};

/**
 * Recharts area chart with the DESIGN.md §2.4 glowing `#14D6E4` gradient.
 * A visually hidden table mirrors the data for screen readers.
 */
export function RevenueChart({ data, showRevenue, showCashFlow }: Props) {
  const { t } = useI18n();
  const instanceId = useId().replace(/:/g, '');
  const revenueGradientId = `revenue-gradient-${instanceId}`;
  const cashGradientId = `cash-gradient-${instanceId}`;

  return (
    <div>
      <div className="mb-3 flex flex-wrap items-center gap-4 text-xs text-charcoal-light">
        {showRevenue ? (
          <span className="inline-flex items-center gap-1.5">
            <span className="h-2.5 w-2.5 rounded-full bg-teal-primary" aria-hidden="true" />
            {t('dashboard.chart.revenue')}
          </span>
        ) : null}
        {showCashFlow ? (
          <span className="inline-flex items-center gap-1.5">
            <span className="h-2.5 w-2.5 rounded-full bg-mint-accent" aria-hidden="true" />
            {t('dashboard.chart.cashFlow')}
          </span>
        ) : null}
      </div>

      <div className="h-72 w-full" aria-hidden="true">
        <ResponsiveContainer width="100%" height="100%" minWidth={0}>
          <AreaChart
            data={[...data]}
            margin={{ top: 8, right: 8, left: 0, bottom: 0 }}
            accessibilityLayer
          >
            <defs>
              <linearGradient id={revenueGradientId} x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor={CHART_COLORS.revenue} stopOpacity={0.4} />
                <stop offset="95%" stopColor={CHART_COLORS.revenue} stopOpacity={0} />
              </linearGradient>
              <linearGradient id={cashGradientId} x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor={CHART_COLORS.cashFlow} stopOpacity={0.3} />
                <stop offset="95%" stopColor={CHART_COLORS.cashFlow} stopOpacity={0} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" stroke={CHART_COLORS.grid} vertical={false} />
            <XAxis
              dataKey="label"
              stroke={CHART_COLORS.axis}
              tickLine={false}
              axisLine={false}
              tick={{ fontSize: 12 }}
            />
            <YAxis
              stroke={CHART_COLORS.axis}
              tickLine={false}
              axisLine={false}
              tick={{ fontSize: 12 }}
              width={72}
              tickFormatter={(value: number) => formatCompactMoney(value)}
            />
            <Tooltip
              cursor={{ stroke: CHART_COLORS.grid }}
              contentStyle={{
                borderRadius: 8,
                borderColor: CHART_COLORS.grid,
                backgroundColor: CHART_COLORS.surface,
                fontSize: 12,
              }}
              labelStyle={{ color: CHART_COLORS.charcoal, fontWeight: 600 }}
              formatter={(value, name) => [formatMoney(String(value)), String(name)]}
            />
            {showRevenue ? (
              <Area
                type="monotone"
                dataKey="revenueValue"
                name={t('dashboard.chart.revenue')}
                stroke={CHART_COLORS.revenue}
                strokeWidth={2}
                fill={`url(#${revenueGradientId})`}
                activeDot={{ r: 4 }}
              />
            ) : null}
            {showCashFlow ? (
              <Area
                type="monotone"
                dataKey="cashFlowValue"
                name={t('dashboard.chart.cashFlow')}
                stroke={CHART_COLORS.cashFlow}
                strokeWidth={2}
                fill={`url(#${cashGradientId})`}
                activeDot={{ r: 4 }}
              />
            ) : null}
          </AreaChart>
        </ResponsiveContainer>
      </div>

      <table className="sr-only">
        <caption>{t('dashboard.chart.tableCaption')}</caption>
        <thead>
          <tr>
            <th scope="col">{t('dashboard.chart.month')}</th>
            <th scope="col">{t('dashboard.chart.revenue')}</th>
            <th scope="col">{t('dashboard.chart.cashFlow')}</th>
          </tr>
        </thead>
        <tbody>
          {data.map((point) => (
            <tr key={point.monthKey}>
              <th scope="row">{point.label}</th>
              <td>{formatMoney(point.revenue)}</td>
              <td>{formatMoney(point.cashFlow)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
