'use client';

import {
  Activity,
  CheckCircle2,
  Factory,
  Fingerprint,
  LockKeyhole,
  ShieldCheck,
  type LucideIcon,
} from 'lucide-react';

import { Logo } from '@/components/brand/Logo';
import { useI18n } from '@/components/providers/LanguageProvider';
import { LanguageSwitcher } from '@/components/shared/LanguageSwitcher';
import { ThemeToggle } from '@/components/shared/ThemeToggle';
import { LoginForm } from '@/features/auth/components/LoginForm';
import type { TranslationKey } from '@/lib/i18n';

type Highlight = {
  titleKey: TranslationKey;
  badgeKey: TranslationKey;
  descriptionKey: TranslationKey;
  icon: LucideIcon;
  tone: 'cyan' | 'mint';
};

const HIGHLIGHTS: readonly Highlight[] = [
  {
    titleKey: 'login.highlight.telemetry.title',
    badgeKey: 'login.highlight.telemetry.badge',
    descriptionKey: 'login.highlight.telemetry.description',
    icon: Activity,
    tone: 'cyan',
  },
  {
    titleKey: 'login.highlight.rbac.title',
    badgeKey: 'login.highlight.rbac.badge',
    descriptionKey: 'login.highlight.rbac.description',
    icon: ShieldCheck,
    tone: 'mint',
  },
  {
    titleKey: 'login.highlight.mes.title',
    badgeKey: 'login.highlight.mes.badge',
    descriptionKey: 'login.highlight.mes.description',
    icon: Factory,
    tone: 'cyan',
  },
];

const CLIENT_LOGOS = [
  'ACME GLOBAL',
  'NEXUS LOGISTICS',
  'STARLIGHT AERO',
  'VERTEX CAPITAL',
  'LUMINA HEALTH',
] as const;

export function LoginView() {
  const { t } = useI18n();

  return (
    <div className="grid min-h-dvh bg-surface lg:grid-cols-[1.04fr_0.96fr]">
      {/* Brand / trust panel */}
      <section
        aria-labelledby="login-brand-title"
        className="relative hidden min-h-dvh flex-col overflow-hidden bg-forest px-10 py-8 text-white lg:flex xl:px-14"
      >
        <div className="bg-radial-forest pointer-events-none absolute inset-0" aria-hidden="true" />
        <div
          className="bg-grid-dark pointer-events-none absolute inset-0 opacity-30"
          aria-hidden="true"
        />
        <div
          className="pointer-events-none absolute -left-32 top-1/3 h-96 w-96 rounded-full bg-cyan/10 blur-3xl"
          aria-hidden="true"
        />

        <div className="relative">
          <Logo
            size="lg"
            theme="dark"
            badge={t('login.badge')}
            subtitle={t('login.brandSubtitle')}
          />
        </div>

        <div className="relative mt-14 max-w-xl xl:mt-20">
          <span className="inline-flex items-center gap-2 rounded-full border border-cyan/30 bg-cyan/10 px-3 py-1.5 text-[10px] font-bold uppercase tracking-[0.16em] text-cyan">
            <Fingerprint className="h-3.5 w-3.5" aria-hidden="true" />
            {t('login.eyebrow')}
          </span>

          <h1
            id="login-brand-title"
            className="mt-6 text-balance text-4xl font-bold leading-[1.06] tracking-[-0.05em] text-white xl:text-5xl"
          >
            {t('login.headline')}
          </h1>

          <p className="mt-5 max-w-lg text-sm leading-6 text-sage-light/75 xl:text-base xl:leading-7">
            {t('login.body')}
          </p>

          <ul className="mt-8 space-y-3">
            {HIGHLIGHTS.map((highlight) => {
              const Icon = highlight.icon;

              return (
                <li
                  key={highlight.titleKey}
                  className="flex items-start gap-3.5 rounded-2xl border border-white/10 bg-white/[0.045] p-4 backdrop-blur transition-colors duration-300 hover:border-cyan/30 hover:bg-white/[0.065]"
                >
                  <span
                    className={
                      highlight.tone === 'mint'
                        ? 'flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-mint-accent/15 text-mint-accent'
                        : 'flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-cyan/15 text-cyan'
                    }
                  >
                    <Icon className="h-5 w-5" aria-hidden="true" />
                  </span>
                  <div className="min-w-0">
                    <div className="flex flex-wrap items-center gap-2">
                      <p className="text-sm font-semibold text-white">{t(highlight.titleKey)}</p>
                      <span
                        className={
                          highlight.tone === 'mint'
                            ? 'rounded-full bg-mint-accent/15 px-2 py-0.5 text-[9px] font-bold uppercase tracking-[0.1em] text-mint-accent'
                            : 'rounded-full bg-cyan/15 px-2 py-0.5 text-[9px] font-bold uppercase tracking-[0.1em] text-cyan'
                        }
                      >
                        {t(highlight.badgeKey)}
                      </span>
                    </div>
                    <p className="mt-1 text-xs leading-5 text-sage-light/60">
                      {t(highlight.descriptionKey)}
                    </p>
                  </div>
                </li>
              );
            })}
          </ul>

          <div
            role="status"
            aria-label={`${t('login.clusterStatus')}: ${t('login.clusterNodes')} | ${t('login.regions')} | ${t('login.sla')}`}
            className="mt-6 grid gap-3 rounded-2xl border border-white/10 bg-white/[0.045] p-4 sm:grid-cols-3"
          >
            <div className="flex items-center gap-2.5">
              <span className="relative flex h-2.5 w-2.5 shrink-0">
                <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-mint-accent opacity-60" />
                <span className="relative inline-flex h-2.5 w-2.5 rounded-full bg-mint-accent" />
              </span>
              <div>
                <p className="text-[10px] font-bold uppercase tracking-[0.12em] text-sage-light/50">
                  {t('login.clusterStatus')}
                </p>
                <p className="font-mono text-xs font-bold text-white">{t('login.clusterNodes')}</p>
              </div>
            </div>
            <div>
              <p className="text-[10px] font-bold uppercase tracking-[0.12em] text-sage-light/50">
                {t('login.coverage')}
              </p>
              <p className="font-mono text-xs font-bold text-white">{t('login.regions')}</p>
            </div>
            <div>
              <p className="text-[10px] font-bold uppercase tracking-[0.12em] text-sage-light/50">
                {t('login.guarantee')}
              </p>
              <p className="font-mono text-xs font-bold text-mint-accent">{t('login.sla')}</p>
            </div>
          </div>
        </div>

        <div className="relative mt-auto pt-10">
          <p className="text-[9px] font-bold uppercase tracking-[0.22em] text-sage-light/45">
            {t('login.trusted')}
          </p>
          <div className="mt-4 flex flex-wrap items-center gap-x-6 gap-y-3 border-t border-white/10 pt-4">
            {CLIENT_LOGOS.map((client) => (
              <span
                key={client}
                className="inline-flex items-center gap-1.5 text-[10px] font-bold uppercase tracking-[0.14em] text-sage-light/55"
              >
                <CheckCircle2 className="h-3 w-3 text-mint-accent/70" aria-hidden="true" />
                {client}
              </span>
            ))}
          </div>
        </div>
      </section>

      {/* Authentication panel */}
      <section aria-labelledby="login-title" className="flex min-h-dvh flex-col bg-surface">
        <header className="flex items-center justify-between gap-3 border-b border-line px-5 py-3 sm:px-8">
          <div className="flex min-w-0 items-center gap-2">
            <span className="relative flex h-2 w-2 shrink-0">
              <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-emerald-400 opacity-60" />
              <span className="relative inline-flex h-2 w-2 rounded-full bg-emerald-500" />
            </span>
            <p className="truncate text-xs font-medium text-charcoal-light">{t('login.gateway')}</p>
          </div>
          <div className="flex shrink-0 items-center gap-2">
            <span className="hidden rounded-md border border-mint-accent/40 bg-mint-accent/15 px-2.5 py-1 font-mono text-[10px] font-bold uppercase tracking-[0.1em] text-on-accent sm:inline-flex">
              TLS 1.3 / E2EE
            </span>
            <LanguageSwitcher />
            <ThemeToggle />
          </div>
        </header>

        <div className="flex flex-1 items-center justify-center px-5 py-8 sm:px-8 sm:py-10">
          <div className="w-full max-w-xl">
            <div className="mb-8 flex items-center justify-between gap-3 lg:hidden">
              <Logo size="md" badge="ERP" />
              <span className="rounded-md border border-line bg-surface-muted px-2 py-1 text-[9px] font-bold uppercase tracking-[0.1em] text-charcoal-light">
                v4.18.2
              </span>
            </div>

            <div className="mb-6 inline-flex items-center gap-2 rounded-lg bg-surface-muted px-3 py-2 text-xs font-semibold text-charcoal-strong">
              <LockKeyhole className="h-4 w-4 text-cyan-deep" aria-hidden="true" />
              {t('login.erpLogin')}
            </div>

            <h2
              id="login-title"
              className="text-balance text-3xl font-bold tracking-[-0.04em] text-charcoal-strong"
            >
              {t('login.accessTitle')}
            </h2>
            <p className="mt-2 text-sm text-charcoal-light">{t('login.accessSubtitle')}</p>

            <div className="mt-7">
              <LoginForm />
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
