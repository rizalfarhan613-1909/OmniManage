import { Inbox, type LucideIcon } from 'lucide-react';
import type { ReactNode } from 'react';

type Props = {
  title: string;
  description: string;
  icon?: LucideIcon;
  action?: ReactNode;
};

/** Required empty state (DESIGN.md §7). */
export function EmptyState({ title, description, icon: Icon = Inbox, action }: Props) {
  return (
    <div className="flex flex-col items-center justify-center rounded-lg border border-dashed border-line bg-surface px-6 py-16 text-center">
      <span className="flex h-12 w-12 items-center justify-center rounded-full bg-surface-muted">
        <Icon className="h-6 w-6 text-charcoal" aria-hidden="true" />
      </span>
      <h2 className="mt-4 text-lg font-semibold text-charcoal">{title}</h2>
      <p className="mt-1 max-w-md text-sm text-charcoal-light">{description}</p>
      {action ? <div className="mt-5">{action}</div> : null}
    </div>
  );
}
