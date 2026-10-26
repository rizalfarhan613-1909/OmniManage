'use client';

import {
  Banknote,
  BookOpenCheck,
  CheckCircle2,
  Circle,
  ReceiptText,
  type LucideIcon,
} from 'lucide-react';

import type { ActivityItem, ActivityKind } from '@/features/dashboard/api/types';
import { t } from '@/lib/i18n';
import { cn } from '@/lib/utils/cn';
import { formatDateTime, formatMoney, formatRelativeTime } from '@/lib/utils/format';

type Props = {
  items: readonly ActivityItem[];
};

const ACTIVITY_ICONS: Record<ActivityKind, LucideIcon> = {
  salesConfirmed: CheckCircle2,
  salesDraft: Circle,
  invoicePaid: Banknote,
  invoiceUnpaid: ReceiptText,
  journalPosted: BookOpenCheck,
};

const ACTIVITY_STYLES: Record<ActivityKind, string> = {
  salesConfirmed: 'bg-mint-accent text-charcoal',
  salesDraft: 'bg-sage-light text-charcoal',
  invoicePaid: 'bg-mint-accent text-charcoal',
  invoiceUnpaid: 'bg-teal-primary/15 text-charcoal',
  journalPosted: 'bg-teal-primary/15 text-charcoal',
};

const ACTIVITY_LABELS: Record<ActivityKind, Parameters<typeof t>[0]> = {
  salesConfirmed: 'dashboard.activity.salesConfirmed',
  salesDraft: 'dashboard.activity.salesDraft',
  invoicePaid: 'dashboard.activity.invoicePaid',
  invoiceUnpaid: 'dashboard.activity.invoiceUnpaid',
  journalPosted: 'dashboard.activity.journalPosted',
};

export function ActivityFeed({ items }: Props) {
  if (items.length === 0) {
    return (
      <p className="py-8 text-center text-sm text-charcoal-light">
        {t('dashboard.activity.empty')}
      </p>
    );
  }

  return (
    <ol className="space-y-4">
      {items.map((item) => {
        const Icon = ACTIVITY_ICONS[item.kind];

        return (
          <li key={item.id} className="flex gap-3">
            <span
              className={cn(
                'mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-full',
                ACTIVITY_STYLES[item.kind],
              )}
            >
              <Icon className="h-4 w-4" aria-hidden="true" />
            </span>
            <div className="min-w-0 flex-1">
              <p className="text-sm text-charcoal">
                {t(ACTIVITY_LABELS[item.kind], { reference: item.reference })}
              </p>
              <div className="mt-1 flex flex-wrap items-center gap-x-3 gap-y-1 text-xs text-charcoal-light">
                <span className="tabular-nums">{formatMoney(item.amount)}</span>
                <time dateTime={item.occurredAt} title={formatDateTime(item.occurredAt)}>
                  {formatRelativeTime(item.occurredAt)}
                </time>
              </div>
            </div>
          </li>
        );
      })}
    </ol>
  );
}
