import { CheckCircle2, LockKeyhole } from 'lucide-react';
import type { Metadata } from 'next';
import { redirect } from 'next/navigation';

import { LoginForm } from '@/features/auth/components/LoginForm';
import { getSession } from '@/lib/auth/session';
import { t } from '@/lib/i18n';

export const metadata: Metadata = {
  title: t('login.title'),
};

const BRAND_POINTS = [
  t('login.brandPoint1'),
  t('login.brandPoint2'),
  t('login.brandPoint3'),
] as const;

export default async function LoginPage() {
  const session = await getSession();

  if (session) {
    redirect('/dashboard');
  }

  return (
    <div className="grid w-full max-w-5xl overflow-hidden rounded-lg border border-sage bg-surface shadow-md lg:grid-cols-[1.1fr_1fr]">
      <section
        aria-labelledby="login-brand-title"
        className="hidden flex-col justify-between bg-charcoal p-10 text-sage lg:flex"
      >
        <div>
          <div className="flex items-center gap-3">
            <span className="flex h-10 w-10 items-center justify-center rounded-md bg-mint-accent text-sm font-bold text-charcoal">
              OM
            </span>
            <span className="text-lg font-semibold text-white">{t('app.name')}</span>
          </div>
          <h1 id="login-brand-title" className="mt-10 text-balance text-2xl font-bold text-white">
            {t('login.brandTagline')}
          </h1>
          <ul className="mt-8 space-y-4">
            {BRAND_POINTS.map((point) => (
              <li key={point} className="flex items-start gap-3 text-sm">
                <CheckCircle2
                  className="mt-0.5 h-4 w-4 shrink-0 text-mint-accent"
                  aria-hidden="true"
                />
                <span>{point}</span>
              </li>
            ))}
          </ul>
        </div>
        <p className="flex items-center gap-2 text-xs text-sage/80">
          <LockKeyhole className="h-3.5 w-3.5" aria-hidden="true" />
          {t('app.description')}
        </p>
      </section>

      <section className="p-6 sm:p-10" aria-labelledby="login-title">
        <div className="mb-6 flex items-center gap-3 lg:hidden">
          <span className="flex h-9 w-9 items-center justify-center rounded-md bg-mint-accent text-sm font-bold text-charcoal">
            OM
          </span>
          <span className="text-base font-semibold text-charcoal">{t('app.name')}</span>
        </div>
        <h2 id="login-title" className="text-2xl font-bold text-charcoal">
          {t('login.title')}
        </h2>
        <p className="mt-1 text-sm text-charcoal-light">{t('login.subtitle')}</p>
        <div className="mt-8">
          <LoginForm />
        </div>
      </section>
    </div>
  );
}
