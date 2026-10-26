import { ShieldX } from 'lucide-react';
import Link from 'next/link';

import { t } from '@/lib/i18n';

/** Required 403 permission state (DESIGN.md §7). */
export function ForbiddenState() {
  return (
    <section
      aria-labelledby="forbidden-title"
      className="mx-auto flex max-w-xl flex-col items-center rounded-lg border border-sage bg-surface px-6 py-12 text-center shadow-sm"
    >
      <span className="bg-danger/10 flex h-12 w-12 items-center justify-center rounded-full">
        <ShieldX className="h-6 w-6 text-danger" aria-hidden="true" />
      </span>
      <h1 id="forbidden-title" className="mt-4 text-lg font-semibold text-charcoal">
        {t('states.forbidden.title')}
      </h1>
      <p className="mt-2 text-sm text-charcoal-light">{t('states.forbidden.body')}</p>
      <Link
        href="/dashboard"
        className="mt-6 inline-flex h-10 items-center justify-center rounded-md border border-sage bg-surface px-4 text-sm font-medium text-charcoal transition-colors duration-300 hover:bg-sage-light focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary focus-visible:ring-offset-2"
      >
        {t('states.moduleNotLicensed.backToDashboard')}
      </Link>
    </section>
  );
}
