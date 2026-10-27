import Link from 'next/link';

import { translate } from '@/lib/i18n';
import { getRequestLocale } from '@/lib/i18n/server';

export default async function NotFound() {
  const locale = await getRequestLocale();

  return (
    <main className="flex min-h-dvh flex-col items-center justify-center bg-bg p-6 text-center">
      <p className="font-mono text-sm text-charcoal-light">404</p>
      <h1 className="mt-2 text-2xl font-bold text-charcoal">
        {translate(locale, 'errors.notFound.title')}
      </h1>
      <p className="mt-2 max-w-md text-sm text-charcoal-light">
        {translate(locale, 'errors.notFound.body')}
      </p>
      <Link
        href="/dashboard"
        className="mt-6 inline-flex h-10 items-center justify-center rounded-md bg-teal-primary px-4 text-sm font-medium text-on-accent transition-colors duration-300 hover:bg-teal-hover focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary focus-visible:ring-offset-2"
      >
        {translate(locale, 'errors.notFound.back')}
      </Link>
    </main>
  );
}
