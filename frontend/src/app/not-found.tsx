import Link from 'next/link';

import { t } from '@/lib/i18n';

export default function NotFound() {
  return (
    <main className="flex min-h-dvh flex-col items-center justify-center bg-bg p-6 text-center">
      <p className="font-mono text-sm text-charcoal-light">404</p>
      <h1 className="mt-2 text-2xl font-bold text-charcoal">{t('errors.notFound.title')}</h1>
      <p className="mt-2 max-w-md text-sm text-charcoal-light">{t('errors.notFound.body')}</p>
      <Link
        href="/dashboard"
        className="mt-6 inline-flex h-10 items-center justify-center rounded-md bg-teal-primary px-4 text-sm font-medium text-charcoal transition-colors duration-300 hover:bg-teal-hover focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary focus-visible:ring-offset-2"
      >
        {t('errors.notFound.back')}
      </Link>
    </main>
  );
}
