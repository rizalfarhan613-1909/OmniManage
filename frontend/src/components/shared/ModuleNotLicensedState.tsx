'use client';

import { LockKeyhole } from 'lucide-react';
import Link from 'next/link';

import { useI18n } from '@/components/providers/LanguageProvider';

type Props = {
  moduleCode?: string;
};

/**
 * Required 403 `PLATFORM.MODULE_NOT_LICENSED` state (DESIGN.md §7).
 * Rendered for deep links into a module that is not active for the tenant.
 */
export function ModuleNotLicensedState({ moduleCode }: Props) {
  const { t } = useI18n();

  return (
    <section
      aria-labelledby="module-not-licensed-title"
      className="mx-auto flex max-w-xl flex-col items-center rounded-lg border border-line bg-surface px-6 py-12 text-center shadow-sm"
    >
      <span className="flex h-12 w-12 items-center justify-center rounded-full bg-surface-muted">
        <LockKeyhole className="h-6 w-6 text-charcoal" aria-hidden="true" />
      </span>
      <h1 id="module-not-licensed-title" className="mt-4 text-lg font-semibold text-charcoal">
        {t('states.moduleNotLicensed.title')}
      </h1>
      <p className="mt-2 text-sm text-charcoal-light">{t('states.moduleNotLicensed.body')}</p>
      {moduleCode ? (
        <p className="mt-4 rounded-md bg-surface-muted px-3 py-1 font-mono text-xs text-charcoal">
          {t('states.moduleNotLicensed.module', { moduleCode })}
        </p>
      ) : null}
      <p className="mt-4 text-sm text-charcoal-light">{t('states.moduleNotLicensed.contact')}</p>
      <Link
        href="/dashboard"
        className="mt-6 inline-flex h-10 items-center justify-center rounded-md bg-teal-primary px-4 text-sm font-medium text-on-accent transition-colors duration-300 hover:bg-teal-hover focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary focus-visible:ring-offset-2"
      >
        {t('states.moduleNotLicensed.backToDashboard')}
      </Link>
    </section>
  );
}
