import { ArrowDownRight, ArrowUpRight, Minus, type LucideIcon } from 'lucide-react';
import Link from 'next/link';

import { cn } from '@/lib/utils/cn';

export type KpiTrend = {
  direction: 'up' | 'down' | 'flat';
  label: string;
};

type Props = {
  label: string;
  value: string;
  helper?: string;
  icon?: LucideIcon;
  trend?: KpiTrend;
  href?: string;
};

const TREND_STYLES: Record<KpiTrend['direction'], string> = {
  up: 'bg-mint-accent text-charcoal',
  down: 'bg-danger/10 text-danger',
  flat: 'bg-surface-muted text-charcoal-light',
};

const TREND_ICONS = {
  up: ArrowUpRight,
  down: ArrowDownRight,
  flat: Minus,
} as const;

const CARD_CLASSES =
  'group flex h-full flex-col rounded-lg border border-line bg-surface p-5 shadow-sm transition-all duration-300 hover:-translate-y-1 hover:shadow-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary focus-visible:ring-offset-2';

/** Dashboard KPI card with the DESIGN.md §2.4 hover-float micro-interaction. */
export function KpiCard({ label, value, helper, icon: Icon, trend, href }: Props) {
  const TrendIcon = trend ? TREND_ICONS[trend.direction] : null;

  const content = (
    <>
      <div className="flex items-start justify-between gap-3">
        <p className="text-xs font-semibold uppercase tracking-wide text-charcoal-light">{label}</p>
        {Icon ? (
          <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-md bg-surface-muted">
            <Icon className="h-4 w-4 text-charcoal" aria-hidden="true" />
          </span>
        ) : null}
      </div>
      <p className="mt-4 text-kpi tabular-nums text-charcoal">{value}</p>
      <div className="mt-auto flex flex-wrap items-center gap-2 pt-3">
        {trend && TrendIcon ? (
          <span
            className={cn(
              'inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-semibold tabular-nums',
              TREND_STYLES[trend.direction],
            )}
          >
            <TrendIcon className="h-3.5 w-3.5" aria-hidden="true" />
            {trend.label}
          </span>
        ) : null}
        {helper ? <p className="text-xs text-charcoal-light">{helper}</p> : null}
      </div>
    </>
  );

  if (href) {
    return (
      <Link href={href} className={CARD_CLASSES} aria-label={`${label}: ${value}`}>
        {content}
      </Link>
    );
  }

  return <article className={CARD_CLASSES}>{content}</article>;
}
