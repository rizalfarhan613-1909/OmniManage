import type { ReactNode } from 'react';

type Props = {
  title: string;
  description?: string;
  actions?: ReactNode;
  status?: ReactNode;
};

/** Standard page header: title, optional status, primary actions (DESIGN.md §3.1). */
export function PageHeader({ title, description, actions, status }: Props) {
  return (
    <header className="flex flex-col gap-4 border-b border-line pb-5 sm:flex-row sm:items-start sm:justify-between">
      <div className="min-w-0">
        <div className="flex flex-wrap items-center gap-3">
          <h1 className="text-balance text-2xl font-bold text-charcoal">{title}</h1>
          {status}
        </div>
        {description ? (
          <p className="mt-1 max-w-3xl text-sm text-charcoal-light">{description}</p>
        ) : null}
      </div>
      {actions ? <div className="flex shrink-0 items-center gap-2">{actions}</div> : null}
    </header>
  );
}
