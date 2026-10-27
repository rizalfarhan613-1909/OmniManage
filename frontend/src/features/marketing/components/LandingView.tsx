'use client';

import {
  Activity,
  ArrowRight,
  ArrowUpRight,
  BadgeCheck,
  Boxes,
  Briefcase,
  Building2,
  CheckCircle2,
  CircleDot,
  Cpu,
  Factory,
  FileText,
  Fingerprint,
  Gauge,
  Globe2,
  HeartPulse,
  Landmark,
  Layers,
  LockKeyhole,
  Network,
  Orbit,
  Play,
  QrCode,
  Radio,
  Server,
  ShieldCheck,
  ShoppingCart,
  Smartphone,
  Truck,
  Users,
  Wallet,
  Zap,
  type LucideIcon,
} from 'lucide-react';
import Link from 'next/link';

import { Logo } from '@/components/brand/Logo';
import { useI18n } from '@/components/providers/LanguageProvider';
import { LanguageSwitcher } from '@/components/shared/LanguageSwitcher';
import { ThemeToggle } from '@/components/shared/ThemeToggle';
import { PricingRoiSection } from '@/features/marketing/components/PricingRoiSection';
import type { TranslationKey } from '@/lib/i18n';

const NAV_LINKS: readonly { labelKey: TranslationKey; href: string }[] = [
  { labelKey: 'landing.nav.modules', href: '#modules' },
  { labelKey: 'landing.nav.architecture', href: '#architecture' },
  { labelKey: 'landing.nav.security', href: '#security' },
  { labelKey: 'landing.nav.pricing', href: '#pricing' },
  { labelKey: 'landing.nav.docs', href: '#footer' },
];

const TRUSTED_OPERATORS: readonly { name: string; icon: LucideIcon }[] = [
  { name: 'ACME GLOBAL', icon: Building2 },
  { name: 'NEXUS LOGISTICS', icon: Boxes },
  { name: 'STARLIGHT AERO', icon: Orbit },
  { name: 'VERTEX CAPITAL', icon: Landmark },
  { name: 'LUMINA HEALTH', icon: HeartPulse },
];

const TELEMETRY_METRICS: readonly {
  labelKey: TranslationKey;
  valueKey: TranslationKey;
  deltaKey: TranslationKey;
  tone: 'mint' | 'cyan';
}[] = [
  {
    labelKey: 'landing.telemetry.revenue',
    valueKey: 'landing.telemetry.revenueValue',
    deltaKey: 'landing.telemetry.revenueDelta',
    tone: 'mint',
  },
  {
    labelKey: 'landing.telemetry.margin',
    valueKey: 'landing.telemetry.marginValue',
    deltaKey: 'landing.telemetry.marginDelta',
    tone: 'mint',
  },
  {
    labelKey: 'landing.telemetry.capacity',
    valueKey: 'landing.telemetry.capacityValue',
    deltaKey: 'landing.telemetry.capacityDelta',
    tone: 'cyan',
  },
  {
    labelKey: 'landing.telemetry.cashflow',
    valueKey: 'landing.telemetry.cashflowValue',
    deltaKey: 'landing.telemetry.cashflowDelta',
    tone: 'mint',
  },
];

const VOLUME_BARS = [
  34, 42, 38, 51, 47, 58, 62, 54, 66, 72, 64, 78, 74, 81, 88, 76, 84, 92, 87, 96, 90, 98, 94, 100,
] as const;

type ModuleCard = {
  index: string;
  nameKey: TranslationKey;
  icon: LucideIcon;
  descriptionKey: TranslationKey;
  tagKeys: readonly TranslationKey[];
};

const MODULES: readonly ModuleCard[] = [
  {
    index: '01',
    nameKey: 'landing.module.inventory.name',
    icon: Boxes,
    descriptionKey: 'landing.module.inventory.description',
    tagKeys: [
      'landing.module.inventory.tag1',
      'landing.module.inventory.tag2',
      'landing.module.inventory.tag3',
    ],
  },
  {
    index: '02',
    nameKey: 'landing.module.finance.name',
    icon: Wallet,
    descriptionKey: 'landing.module.finance.description',
    tagKeys: [
      'landing.module.finance.tag1',
      'landing.module.finance.tag2',
      'landing.module.finance.tag3',
    ],
  },
  {
    index: '03',
    nameKey: 'landing.module.hris.name',
    icon: Users,
    descriptionKey: 'landing.module.hris.description',
    tagKeys: ['landing.module.hris.tag1', 'landing.module.hris.tag2', 'landing.module.hris.tag3'],
  },
  {
    index: '04',
    nameKey: 'landing.module.manufacturing.name',
    icon: Factory,
    descriptionKey: 'landing.module.manufacturing.description',
    tagKeys: [
      'landing.module.manufacturing.tag1',
      'landing.module.manufacturing.tag2',
      'landing.module.manufacturing.tag3',
    ],
  },
  {
    index: '05',
    nameKey: 'landing.module.crm.name',
    icon: ShoppingCart,
    descriptionKey: 'landing.module.crm.description',
    tagKeys: ['landing.module.crm.tag1', 'landing.module.crm.tag2', 'landing.module.crm.tag3'],
  },
  {
    index: '06',
    nameKey: 'landing.module.procurement.name',
    icon: Truck,
    descriptionKey: 'landing.module.procurement.description',
    tagKeys: [
      'landing.module.procurement.tag1',
      'landing.module.procurement.tag2',
      'landing.module.procurement.tag3',
    ],
  },
  {
    index: '07',
    nameKey: 'landing.module.projects.name',
    icon: Briefcase,
    descriptionKey: 'landing.module.projects.description',
    tagKeys: [
      'landing.module.projects.tag1',
      'landing.module.projects.tag2',
      'landing.module.projects.tag3',
    ],
  },
  {
    index: '08',
    nameKey: 'landing.module.assets.name',
    icon: QrCode,
    descriptionKey: 'landing.module.assets.description',
    tagKeys: [
      'landing.module.assets.tag1',
      'landing.module.assets.tag2',
      'landing.module.assets.tag3',
    ],
  },
  {
    index: '09',
    nameKey: 'landing.module.platform.name',
    icon: Layers,
    descriptionKey: 'landing.module.platform.description',
    tagKeys: [
      'landing.module.platform.tag1',
      'landing.module.platform.tag2',
      'landing.module.platform.tag3',
    ],
  },
  {
    index: '10',
    nameKey: 'landing.module.audit.name',
    icon: Fingerprint,
    descriptionKey: 'landing.module.audit.description',
    tagKeys: [
      'landing.module.audit.tag1',
      'landing.module.audit.tag2',
      'landing.module.audit.tag3',
    ],
  },
];

const INFRASTRUCTURE: readonly {
  icon: LucideIcon;
  titleKey: TranslationKey;
  descriptionKey: TranslationKey;
  tagKeys: readonly TranslationKey[];
  metricKey: TranslationKey;
}[] = [
  {
    icon: Cpu,
    titleKey: 'landing.architecture.dotnet.title',
    descriptionKey: 'landing.architecture.dotnet.description',
    tagKeys: [
      'landing.architecture.dotnet.tag1',
      'landing.architecture.dotnet.tag2',
      'landing.architecture.dotnet.tag3',
    ],
    metricKey: 'landing.architecture.dotnet.metric',
  },
  {
    icon: ShieldCheck,
    titleKey: 'landing.architecture.rls.title',
    descriptionKey: 'landing.architecture.rls.description',
    tagKeys: [
      'landing.architecture.rls.tag1',
      'landing.architecture.rls.tag2',
      'landing.architecture.rls.tag3',
    ],
    metricKey: 'landing.architecture.rls.metric',
  },
  {
    icon: Smartphone,
    titleKey: 'landing.architecture.mobile.title',
    descriptionKey: 'landing.architecture.mobile.description',
    tagKeys: [
      'landing.architecture.mobile.tag1',
      'landing.architecture.mobile.tag2',
      'landing.architecture.mobile.tag3',
    ],
    metricKey: 'landing.architecture.mobile.metric',
  },
];

const ARCHITECTURE_STATS: readonly {
  labelKey: TranslationKey;
  valueKey: TranslationKey;
  icon: LucideIcon;
}[] = [
  {
    labelKey: 'landing.architecture.stats.uptime',
    valueKey: 'landing.architecture.stats.uptimeValue',
    icon: Gauge,
  },
  {
    labelKey: 'landing.architecture.stats.gateway',
    valueKey: 'landing.architecture.stats.gatewayValue',
    icon: Network,
  },
  {
    labelKey: 'landing.architecture.stats.events',
    valueKey: 'landing.architecture.stats.eventsValue',
    icon: Fingerprint,
  },
  {
    labelKey: 'landing.architecture.stats.encryption',
    valueKey: 'landing.architecture.stats.encryptionValue',
    icon: LockKeyhole,
  },
];

const FOOTER_COLUMNS: readonly {
  titleKey: TranslationKey;
  links: readonly TranslationKey[];
}[] = [
  {
    titleKey: 'landing.footer.platform',
    links: [
      'landing.footer.link.executiveBi',
      'landing.footer.link.licensing',
      'landing.footer.link.auditLogs',
      'landing.footer.link.apiDocs',
      'landing.footer.link.systemStatus',
    ],
  },
  {
    titleKey: 'landing.footer.modules',
    links: [
      'landing.module.inventory.name',
      'landing.module.finance.name',
      'landing.module.hris.name',
      'landing.module.manufacturing.name',
      'landing.module.crm.name',
    ],
  },
  {
    titleKey: 'landing.footer.architecture',
    links: [
      'landing.footer.link.dotnetCore',
      'landing.footer.link.rustEngine',
      'landing.footer.link.rlsIsolation',
      'landing.footer.link.scadaBridge',
      'landing.footer.link.eventStream',
    ],
  },
  {
    titleKey: 'landing.footer.governance',
    links: [
      'landing.footer.link.soc2',
      'landing.footer.link.hipaa',
      'landing.footer.link.iso',
      'landing.footer.link.merkle',
      'landing.footer.link.trustCenter',
    ],
  },
  {
    titleKey: 'landing.footer.company',
    links: [
      'landing.footer.link.about',
      'landing.footer.link.careers',
      'landing.footer.link.contact',
      'landing.footer.link.partners',
      'landing.footer.link.legal',
    ],
  },
];

const COMPLIANCE_BADGES = ['SOC 2 Type II', 'HIPAA', 'ISO 27001'] as const;

function SectionEyebrow({ children }: { children: string }) {
  return (
    <span className="stitch-pill border-cyan/30 bg-cyan/10 text-charcoal-strong">
      <CircleDot className="h-3.5 w-3.5 text-cyan-deep" aria-hidden="true" />
      {children}
    </span>
  );
}

export function LandingView() {
  const { t } = useI18n();

  return (
    <div className="min-h-dvh bg-bg">
      {/* Top status rail */}
      <div className="bg-sidebar text-sidebar-muted">
        <div className="landing-shell flex h-9 items-center justify-between gap-4 text-[10px] font-semibold uppercase tracking-[0.16em]">
          <span className="inline-flex min-w-0 items-center gap-2 truncate">
            <span className="relative flex h-2 w-2 shrink-0">
              <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-mint-accent opacity-60" />
              <span className="relative inline-flex h-2 w-2 rounded-full bg-mint-accent" />
            </span>
            <span className="truncate">
              {t('landing.rail.product')} <span className="text-sidebar-muted/50">|</span>{' '}
              {t('landing.rail.tagline')}
            </span>
          </span>
          <span className="hidden items-center gap-4 text-sidebar-muted/70 sm:flex">
            <span className="inline-flex items-center gap-1.5">
              <Server className="h-3 w-3 text-mint-accent" aria-hidden="true" />
              {t('landing.rail.nodes')}
            </span>
            <span className="inline-flex items-center gap-1.5">
              <LockKeyhole className="h-3 w-3 text-cyan" aria-hidden="true" />
              {t('landing.rail.security')}
            </span>
          </span>
        </div>
      </div>

      {/* Navigation */}
      <header className="sticky top-0 z-40 border-b border-line/80 bg-bg/90 backdrop-blur-xl">
        <div className="landing-shell flex h-16 items-center justify-between gap-4">
          <Link
            href="/"
            aria-label={t('landing.nav.home')}
            className="rounded-lg focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary"
          >
            <Logo size="md" badge="ERP" />
          </Link>

          <nav aria-label={t('landing.nav.aria')} className="hidden items-center gap-1 lg:flex">
            {NAV_LINKS.map((link) => (
              <a
                key={link.labelKey}
                href={link.href}
                className="rounded-lg px-3 py-2 text-sm font-medium text-charcoal-light transition-colors hover:bg-surface-muted hover:text-charcoal-strong"
              >
                {t(link.labelKey)}
              </a>
            ))}
          </nav>

          <div className="flex items-center gap-2">
            <LanguageSwitcher />
            <ThemeToggle />
            <Link
              href="/login"
              className="hidden rounded-xl px-3 py-2 text-sm font-semibold text-charcoal-strong transition-colors hover:bg-surface-muted sm:inline-flex"
            >
              {t('landing.nav.signIn')}
            </Link>
            <Link
              href="/login"
              className="inline-flex h-10 items-center gap-2 rounded-xl bg-cyan px-3 text-sm font-semibold text-on-accent transition-colors hover:bg-cyan-deep hover:text-white sm:px-4"
            >
              {t('landing.nav.launch')}
              <ArrowRight className="h-4 w-4" aria-hidden="true" />
            </Link>
          </div>
        </div>
      </header>

      {/* Hero */}
      <section className="relative overflow-hidden">
        <div className="bg-radial-mint pointer-events-none absolute inset-0" aria-hidden="true" />
        <div
          className="bg-grid-light pointer-events-none absolute inset-0 opacity-40 [mask-image:linear-gradient(to_bottom,black,transparent_85%)]"
          aria-hidden="true"
        />

        <div className="landing-shell relative pb-16 pt-16 sm:pt-20 lg:pb-24">
          <div className="mx-auto max-w-4xl text-center">
            <SectionEyebrow>{t('landing.hero.eyebrow')}</SectionEyebrow>

            <h1 className="mt-6 text-balance text-4xl font-bold leading-[1.04] tracking-[-0.055em] text-charcoal-strong sm:text-6xl lg:text-7xl">
              {t('landing.hero.titleLead')}{' '}
              <span className="text-gradient-cyan">{t('landing.hero.titleAccent')}</span>
            </h1>

            <p className="mx-auto mt-6 max-w-3xl text-balance text-base leading-7 text-charcoal-light sm:text-lg">
              {t('landing.hero.description')}
            </p>

            <div className="mt-9 flex flex-col items-center justify-center gap-3 sm:flex-row">
              <Link
                href="/login"
                className="inline-flex h-12 w-full items-center justify-center gap-2 rounded-xl bg-cyan px-6 text-sm font-bold text-on-accent shadow-lg shadow-cyan/20 transition-all duration-300 hover:-translate-y-0.5 hover:bg-cyan-deep hover:text-white sm:w-auto"
              >
                <Play className="h-4 w-4 fill-current" aria-hidden="true" />
                {t('landing.hero.demo')}
              </Link>
              <a
                href="#architecture"
                className="inline-flex h-12 w-full items-center justify-center gap-2 rounded-xl border border-line bg-surface px-6 text-sm font-semibold text-charcoal-strong transition-all duration-300 hover:-translate-y-0.5 hover:border-cyan/50 hover:bg-cyan/5 sm:w-auto"
              >
                <FileText className="h-4 w-4 text-cyan-deep" aria-hidden="true" />
                {t('landing.hero.architecture')}
              </a>
            </div>

            <p className="mt-5 flex flex-wrap items-center justify-center gap-x-5 gap-y-2 text-[11px] font-medium uppercase tracking-[0.12em] text-charcoal-light">
              <span className="inline-flex items-center gap-1.5">
                <CheckCircle2 className="h-3.5 w-3.5 text-cyan-deep" aria-hidden="true" />
                {t('landing.hero.noCard')}
              </span>
              <span className="inline-flex items-center gap-1.5">
                <CheckCircle2 className="h-3.5 w-3.5 text-cyan-deep" aria-hidden="true" />
                {t('landing.hero.sandbox')}
              </span>
              <span className="inline-flex items-center gap-1.5">
                <CheckCircle2 className="h-3.5 w-3.5 text-cyan-deep" aria-hidden="true" />
                {t('landing.hero.soc')}
              </span>
            </p>
          </div>

          {/* Trusted operators */}
          <div className="mx-auto mt-14 max-w-5xl">
            <p className="text-center text-[10px] font-bold uppercase tracking-[0.24em] text-charcoal-light">
              {t('landing.hero.trusted')}
            </p>
            <div className="mt-5 flex flex-wrap items-center justify-center gap-x-8 gap-y-4">
              {TRUSTED_OPERATORS.map((operator) => {
                const Icon = operator.icon;

                return (
                  <span
                    key={operator.name}
                    className="inline-flex items-center gap-2 text-[11px] font-bold uppercase tracking-[0.16em] text-charcoal-light/70 transition-colors hover:text-charcoal-strong"
                  >
                    <Icon className="h-4 w-4" aria-hidden="true" />
                    {operator.name}
                  </span>
                );
              })}
            </div>
          </div>

          {/* Live telemetry preview */}
          <div className="relative mx-auto mt-12 max-w-6xl">
            <div
              className="pointer-events-none absolute -inset-6 rounded-[2rem] bg-cyan/10 blur-3xl"
              aria-hidden="true"
            />
            <div className="relative overflow-hidden rounded-2xl border border-line bg-surface shadow-md">
              <div className="flex flex-wrap items-center justify-between gap-3 border-b border-line bg-surface-muted px-5 py-4 sm:px-6">
                <div className="flex items-center gap-3">
                  <span className="relative flex h-2.5 w-2.5">
                    <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-emerald-400 opacity-60" />
                    <span className="relative inline-flex h-2.5 w-2.5 rounded-full bg-emerald-500" />
                  </span>
                  <div>
                    <p className="text-sm font-semibold text-charcoal-strong">
                      {t('landing.telemetry.title')}
                    </p>
                    <p className="text-2xs text-charcoal-light">
                      {t('landing.telemetry.subtitle')}
                    </p>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <span className="inline-flex items-center gap-1.5 rounded-full bg-mint-accent/40 px-2.5 py-1 text-[10px] font-bold uppercase tracking-[0.12em] text-on-accent">
                    <Activity className="h-3 w-3" aria-hidden="true" />
                    {t('landing.telemetry.sync')}
                  </span>
                  <span className="hidden rounded-full border border-line bg-surface px-2.5 py-1 text-[10px] font-semibold uppercase tracking-[0.12em] text-charcoal-light sm:inline-flex">
                    {t('landing.telemetry.modulesActive')}
                  </span>
                </div>
              </div>

              <div className="grid gap-0 lg:grid-cols-[1.55fr_1fr]">
                <div className="border-b border-line p-5 sm:p-6 lg:border-b-0 lg:border-r">
                  <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
                    {TELEMETRY_METRICS.map((metric) => (
                      <div key={metric.labelKey}>
                        <p className="text-3xs font-bold uppercase tracking-[0.12em] text-charcoal-light">
                          {t(metric.labelKey)}
                        </p>
                        <p className="mt-1.5 text-lg font-bold tracking-tight text-charcoal-strong sm:text-xl">
                          {t(metric.valueKey)}
                        </p>
                        <p
                          className={
                            metric.tone === 'cyan'
                              ? 'mt-1 text-[10px] font-semibold text-cyan-deep'
                              : 'mt-1 text-[10px] font-semibold text-emerald-600 dark:text-emerald-400'
                          }
                        >
                          {t(metric.deltaKey)}
                        </p>
                      </div>
                    ))}
                  </div>

                  <div className="mt-7">
                    <div className="flex items-end justify-between gap-3">
                      <div>
                        <p className="text-xs font-semibold text-charcoal">
                          {t('landing.telemetry.volumeTitle')}
                        </p>
                        <p className="text-2xs text-charcoal-light">
                          {t('landing.telemetry.volumeSubtitle')}
                        </p>
                      </div>
                      <div className="text-right">
                        <p className="font-mono text-sm font-bold text-charcoal-strong">
                          {t('landing.telemetry.volumeValue')}
                        </p>
                        <p className="text-[10px] font-semibold text-emerald-600 dark:text-emerald-400">
                          {t('landing.telemetry.volumeDelta')}
                        </p>
                      </div>
                    </div>
                    <div className="mt-4 flex h-28 items-end gap-1.5" aria-hidden="true">
                      {VOLUME_BARS.map((value, index) => (
                        <div
                          key={`${value}-${index}`}
                          className="flex-1 origin-bottom animate-bar-rise rounded-t-[3px] bg-gradient-to-t from-cyan/35 via-cyan/70 to-cyan"
                          style={{ height: `${value}%`, animationDelay: `${index * 24}ms` }}
                        />
                      ))}
                    </div>
                    <div className="mt-2 flex justify-between font-mono text-[9px] uppercase tracking-wider text-charcoal-light">
                      <span>00:00</span>
                      <span>06:00</span>
                      <span>12:00</span>
                      <span>18:00</span>
                      <span>24:00</span>
                    </div>
                  </div>
                </div>

                <div className="flex flex-col justify-between gap-4 bg-surface-muted p-5 sm:p-6">
                  <div>
                    <p className="text-2xs font-bold uppercase tracking-[0.16em] text-charcoal-light">
                      {t('landing.telemetry.signals')}
                    </p>
                    <ul className="mt-4 space-y-3">
                      {[
                        {
                          labelKey: 'landing.telemetry.signal.cluster' as const,
                          valueKey: 'landing.telemetry.signal.clusterValue' as const,
                          icon: Globe2,
                        },
                        {
                          labelKey: 'landing.telemetry.signal.mrp' as const,
                          valueKey: 'landing.telemetry.signal.mrpValue' as const,
                          icon: Boxes,
                        },
                        {
                          labelKey: 'landing.telemetry.signal.ledger' as const,
                          valueKey: 'landing.telemetry.signal.ledgerValue' as const,
                          icon: Wallet,
                        },
                        {
                          labelKey: 'landing.telemetry.signal.audit' as const,
                          valueKey: 'landing.telemetry.signal.auditValue' as const,
                          icon: Fingerprint,
                        },
                      ].map((signal) => {
                        const Icon = signal.icon;

                        return (
                          <li
                            key={signal.labelKey}
                            className="flex items-center justify-between gap-3 rounded-xl border border-line bg-surface px-3 py-2.5"
                          >
                            <span className="inline-flex min-w-0 items-center gap-2 text-xs font-medium text-charcoal">
                              <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-lg bg-cyan/10 text-cyan-deep">
                                <Icon className="h-3.5 w-3.5" aria-hidden="true" />
                              </span>
                              <span className="truncate">{t(signal.labelKey)}</span>
                            </span>
                            <span className="shrink-0 font-mono text-[10px] font-bold text-charcoal-strong">
                              {t(signal.valueKey)}
                            </span>
                          </li>
                        );
                      })}
                    </ul>
                  </div>
                  <div className="rounded-xl border border-cyan/30 bg-cyan/10 px-3 py-2.5">
                    <p className="text-3xs font-bold uppercase tracking-[0.14em] text-charcoal-light">
                      {t('landing.telemetry.heartbeat')}
                    </p>
                    <p className="mt-1 flex items-center gap-2 text-xs font-semibold text-charcoal-strong">
                      <Zap className="h-3.5 w-3.5 text-cyan-deep" aria-hidden="true" />
                      {t('landing.telemetry.heartbeatValue')}
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* 10 native modules */}
      <section id="modules" className="border-y border-line bg-surface py-20 sm:py-24">
        <div className="landing-shell">
          <div className="flex flex-col gap-6 lg:flex-row lg:items-end lg:justify-between">
            <div className="max-w-3xl">
              <SectionEyebrow>{t('landing.modules.eyebrow')}</SectionEyebrow>
              <h2 className="mt-5 text-balance text-3xl font-bold tracking-[-0.04em] text-charcoal-strong sm:text-4xl">
                {t('landing.modules.title')}
              </h2>
              <p className="mt-4 text-base leading-7 text-charcoal-light">
                {t('landing.modules.description')}
              </p>
            </div>
            <div className="flex flex-wrap gap-2">
              {[
                'landing.modules.filter.all',
                'landing.modules.filter.core',
                'landing.modules.filter.supply',
                'landing.modules.filter.governance',
              ].map((filter, index) => (
                <span
                  key={filter}
                  className={
                    index === 0
                      ? 'rounded-full bg-sidebar px-3 py-1.5 text-[11px] font-semibold text-sidebar-fg'
                      : 'rounded-full border border-line bg-surface px-3 py-1.5 text-[11px] font-semibold text-charcoal-light'
                  }
                >
                  {t(filter as TranslationKey)}
                </span>
              ))}
            </div>
          </div>

          <div className="mt-10 grid gap-4 md:grid-cols-2">
            {MODULES.map((module) => {
              const Icon = module.icon;

              return (
                <article
                  key={module.index}
                  className="group relative overflow-hidden rounded-2xl border border-line bg-surface p-5 transition-all duration-300 hover:-translate-y-1 hover:border-cyan/40 hover:shadow-md sm:p-6"
                >
                  <span
                    className="pointer-events-none absolute right-4 top-3 text-5xl font-bold tracking-tighter text-charcoal-strong/[0.045]"
                    aria-hidden="true"
                  >
                    {module.index}
                  </span>
                  <div className="flex items-start gap-3">
                    <span className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl border border-line bg-surface-muted text-charcoal-strong transition-colors duration-300 group-hover:border-cyan/40 group-hover:bg-cyan/10">
                      <Icon className="h-5 w-5" aria-hidden="true" />
                    </span>
                    <div className="min-w-0">
                      <p className="font-mono text-[10px] font-semibold uppercase tracking-[0.18em] text-charcoal-light">
                        {t('landing.modules.label', { index: module.index })}
                      </p>
                      <h3 className="mt-0.5 text-base font-bold text-charcoal-strong">
                        {t(module.nameKey)}
                      </h3>
                    </div>
                  </div>
                  <p className="mt-4 text-sm leading-6 text-charcoal-light">
                    {t(module.descriptionKey)}
                  </p>
                  <div className="mt-4 flex flex-wrap gap-2">
                    {module.tagKeys.map((tagKey) => (
                      <span
                        key={tagKey}
                        className="inline-flex items-center gap-1 rounded-md border border-line bg-surface-muted px-2 py-1 text-[10px] font-semibold text-charcoal-light"
                      >
                        <BadgeCheck className="h-3 w-3 text-cyan-deep" aria-hidden="true" />
                        {t(tagKey)}
                      </span>
                    ))}
                  </div>
                </article>
              );
            })}
          </div>

          <div className="mt-8 flex flex-col items-center justify-between gap-4 rounded-2xl border border-line bg-surface-muted px-5 py-4 sm:flex-row">
            <p className="text-sm text-charcoal-light">
              <span className="font-semibold text-charcoal-strong">
                {t('landing.modules.license.title')}
              </span>{' '}
              {t('landing.modules.license.body')}
            </p>
            <Link
              href="/login"
              className="inline-flex items-center gap-2 text-sm font-semibold text-charcoal-strong transition-colors hover:text-cyan-deep"
            >
              {t('landing.modules.license.cta')}
              <ArrowUpRight className="h-4 w-4" aria-hidden="true" />
            </Link>
          </div>
        </div>
      </section>

      {/* Infrastructure */}
      <section
        id="architecture"
        className="relative overflow-hidden bg-forest py-20 text-white sm:py-24"
      >
        <div className="bg-radial-forest pointer-events-none absolute inset-0" aria-hidden="true" />
        <div
          className="bg-grid-dark pointer-events-none absolute inset-0 opacity-40"
          aria-hidden="true"
        />
        <div className="landing-shell relative">
          <div className="mx-auto max-w-3xl text-center">
            <span className="inline-flex items-center gap-2 rounded-full border border-white/15 bg-white/5 px-3 py-1 text-[10px] font-bold uppercase tracking-[0.18em] text-mint-accent">
              <ShieldCheck className="h-3.5 w-3.5" aria-hidden="true" />
              {t('landing.architecture.eyebrow')}
            </span>
            <h2 className="mt-5 text-balance text-3xl font-bold tracking-[-0.04em] sm:text-4xl">
              {t('landing.architecture.title')}
            </h2>
            <p className="mt-4 text-base leading-7 text-sage-light/75">
              {t('landing.architecture.description')}
            </p>
          </div>

          <div className="mt-12 grid gap-4 lg:grid-cols-3">
            {INFRASTRUCTURE.map((item) => {
              const Icon = item.icon;

              return (
                <article
                  key={item.titleKey}
                  className="flex flex-col rounded-2xl border border-white/10 bg-white/[0.045] p-6 backdrop-blur transition-colors duration-300 hover:border-cyan/40 hover:bg-white/[0.07]"
                >
                  <div className="flex items-start justify-between gap-3">
                    <span className="flex h-11 w-11 items-center justify-center rounded-xl border border-cyan/25 bg-cyan/10 text-cyan">
                      <Icon className="h-5 w-5" aria-hidden="true" />
                    </span>
                    <span className="rounded-full border border-white/10 bg-white/5 px-2.5 py-1 font-mono text-[10px] font-semibold text-mint-accent">
                      {t(item.metricKey)}
                    </span>
                  </div>
                  <h3 className="mt-5 text-lg font-bold tracking-tight">{t(item.titleKey)}</h3>
                  <p className="mt-3 flex-1 text-sm leading-6 text-sage-light/70">
                    {t(item.descriptionKey)}
                  </p>
                  <div className="mt-5 flex flex-wrap gap-2">
                    {item.tagKeys.map((tagKey) => (
                      <span
                        key={tagKey}
                        className="rounded-md border border-white/10 bg-white/5 px-2 py-1 text-[10px] font-semibold text-sage-light/80"
                      >
                        {t(tagKey)}
                      </span>
                    ))}
                  </div>
                </article>
              );
            })}
          </div>

          <div
            id="security"
            className="mt-8 grid gap-3 rounded-2xl border border-white/10 bg-white/[0.045] p-4 sm:grid-cols-2 lg:grid-cols-4"
          >
            {ARCHITECTURE_STATS.map((stat) => {
              const Icon = stat.icon;

              return (
                <div
                  key={stat.labelKey}
                  className="flex items-center gap-3 rounded-xl bg-forest-950/40 px-3 py-3"
                >
                  <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-mint-accent/10 text-mint-accent">
                    <Icon className="h-4 w-4" aria-hidden="true" />
                  </span>
                  <div>
                    <p className="text-3xs font-bold uppercase tracking-[0.14em] text-sage-light/60">
                      {t(stat.labelKey)}
                    </p>
                    <p className="font-mono text-sm font-bold text-white">{t(stat.valueKey)}</p>
                  </div>
                </div>
              );
            })}
          </div>

          <div className="mt-8 flex flex-wrap items-center justify-center gap-3">
            {COMPLIANCE_BADGES.map((badge) => (
              <span
                key={badge}
                className="inline-flex items-center gap-1.5 rounded-full border border-mint-accent/25 bg-mint-accent/10 px-3 py-1.5 text-[11px] font-semibold text-mint-accent"
              >
                <CheckCircle2 className="h-3.5 w-3.5" aria-hidden="true" />
                {badge}
              </span>
            ))}
          </div>
        </div>
      </section>

      <PricingRoiSection />

      {/* Footer */}
      <footer id="footer" className="bg-forest-950 text-sage-light">
        <div className="landing-shell py-14">
          <div className="grid gap-10 lg:grid-cols-[1.2fr_2.8fr]">
            <div>
              <Logo size="md" theme="dark" badge="ERP" />
              <p className="mt-4 max-w-xs text-sm leading-6 text-sage-light/65">
                {t('landing.footer.tagline')}
              </p>
              <div className="mt-5 flex flex-wrap items-center gap-2">
                {COMPLIANCE_BADGES.map((badge) => (
                  <span
                    key={badge}
                    className="inline-flex items-center gap-1 rounded-md border border-white/10 bg-white/5 px-2 py-1 text-[10px] font-semibold text-sage-light/80"
                  >
                    <BadgeCheck className="h-3 w-3 text-mint-accent" aria-hidden="true" />
                    {badge}
                  </span>
                ))}
              </div>
              <div className="mt-6 inline-flex items-center gap-2 rounded-full border border-mint-accent/20 bg-mint-accent/10 px-3 py-1.5 text-[10px] font-semibold uppercase tracking-[0.14em] text-mint-accent">
                <span className="relative flex h-2 w-2">
                  <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-mint-accent opacity-60" />
                  <span className="relative inline-flex h-2 w-2 rounded-full bg-mint-accent" />
                </span>
                {t('landing.footer.systems')}
              </div>
            </div>

            <div className="grid grid-cols-2 gap-8 sm:grid-cols-3 lg:grid-cols-5">
              {FOOTER_COLUMNS.map((column) => (
                <div key={column.titleKey}>
                  <p className="text-[10px] font-bold uppercase tracking-[0.18em] text-sage-light/50">
                    {t(column.titleKey)}
                  </p>
                  <ul className="mt-3 space-y-2">
                    {column.links.map((linkKey) => (
                      <li key={linkKey}>
                        <a
                          href="#footer"
                          className="text-xs text-sage-light/70 transition-colors hover:text-white"
                        >
                          {t(linkKey)}
                        </a>
                      </li>
                    ))}
                  </ul>
                </div>
              ))}
            </div>
          </div>

          <div className="mt-12 flex flex-col gap-3 border-t border-white/10 pt-6 text-[10px] text-sage-light/50 sm:flex-row sm:items-center sm:justify-between">
            <p>{t('landing.footer.rights')}</p>
            <p className="flex flex-wrap items-center gap-x-4 gap-y-2">
              <span>{t('landing.footer.privacy')}</span>
              <span>{t('landing.footer.terms')}</span>
              <span>{t('landing.footer.security')}</span>
              <span>{t('landing.footer.dpa')}</span>
              <span className="inline-flex items-center gap-1.5 text-sage-light/70">
                <Radio className="h-3 w-3 text-mint-accent" aria-hidden="true" />
                {t('landing.footer.status')}
              </span>
            </p>
          </div>
        </div>
      </footer>
    </div>
  );
}
