import type { ReactNode } from 'react';

import { cn } from '@/lib/utils/cn';

type Props = {
  title: string;
  description?: string;
  action?: ReactNode;
  footer?: ReactNode;
  children: ReactNode;
  className?: string;
};

/** Standard surface for charts and dense dashboard panels. */
export function ChartCard({ title, description, action, footer, children, className }: Props) {
  return (
    <section
      aria-label={title}
      className={cn(
        'flex h-full flex-col rounded-lg border border-sage bg-surface p-5 shadow-sm',
        className,
      )}
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="min-w-0">
          <h2 className="text-lg font-semibold text-charcoal">{title}</h2>
          {description ? (
            <p className="mt-1 max-w-2xl text-sm text-charcoal-light">{description}</p>
          ) : null}
        </div>
        {action ? <div className="shrink-0">{action}</div> : null}
      </div>
      <div className="mt-5 min-w-0 flex-1">{children}</div>
      {footer ? <div className="mt-4 border-t border-sage pt-3">{footer}</div> : null}
    </section>
  );
}
