import { ChevronRight } from 'lucide-react';

import { t } from '@/lib/i18n';

import { buildPayloadDiff } from './payload-diff';

type Props = {
  before: Record<string, unknown> | null;
  after: Record<string, unknown> | null;
};

function formatValue(value: unknown): string {
  if (value === null || value === undefined) {
    return t('admin.security.audit.emptyValue');
  }

  if (typeof value === 'string') {
    return value;
  }

  if (typeof value === 'number' || typeof value === 'boolean') {
    return String(value);
  }

  try {
    return JSON.stringify(value) ?? t('admin.security.audit.emptyValue');
  } catch {
    return String(value);
  }
}

/** Collapsible before/after payload diff for the audit trail. */
export function PayloadDiff({ before, after }: Props) {
  const changes = buildPayloadDiff(before, after);

  if (changes.length === 0) {
    return (
      <span className="text-xs text-charcoal-light">{t('admin.security.audit.noChanges')}</span>
    );
  }

  return (
    <details className="group">
      <summary className="inline-flex cursor-pointer list-none items-center gap-1 rounded-sm text-xs font-medium text-teal-hover transition-colors duration-300 hover:text-charcoal focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary">
        <ChevronRight
          className="h-3.5 w-3.5 transition-transform duration-200 group-open:rotate-90"
          aria-hidden="true"
        />
        {t('admin.security.audit.viewDiff')}
        <span className="text-charcoal-light">
          ({t('admin.security.audit.changes', { count: changes.length })})
        </span>
      </summary>

      <dl className="mt-2 max-w-md space-y-2">
        {changes.map((change) => (
          <div key={change.field} className="rounded-md bg-sage-light/60 px-2.5 py-2">
            <dt className="font-mono text-[11px] font-semibold text-charcoal">{change.field}</dt>
            <dd className="mt-1 grid gap-1.5 sm:grid-cols-2">
              <div className="min-w-0">
                <span className="text-[10px] font-semibold uppercase tracking-wide text-charcoal-light">
                  {t('admin.security.audit.before')}
                </span>
                <code className="mt-0.5 block break-all font-mono text-[11px] text-charcoal">
                  {formatValue(change.before)}
                </code>
              </div>
              <div className="min-w-0">
                <span className="text-[10px] font-semibold uppercase tracking-wide text-charcoal-light">
                  {t('admin.security.audit.after')}
                </span>
                <code className="mt-0.5 block break-all font-mono text-[11px] text-charcoal">
                  {formatValue(change.after)}
                </code>
              </div>
            </dd>
          </div>
        ))}
      </dl>
    </details>
  );
}
