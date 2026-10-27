'use client';

import { ArrowRight, CheckCircle2, Gauge, Sparkles, TrendingUp, Users, Wallet } from 'lucide-react';
import { useState } from 'react';

import { useI18n } from '@/components/providers/LanguageProvider';
import type { TranslationKey } from '@/lib/i18n';
import { cn } from '@/lib/utils/cn';

type Tier = {
  nameKey: TranslationKey;
  price: string;
  descriptionKey: TranslationKey;
  featureKeys: readonly TranslationKey[];
  ctaKey: TranslationKey;
  featured?: boolean;
};

const TIERS: readonly Tier[] = [
  {
    nameKey: 'landing.pricing.starter.name',
    price: '$9,400',
    descriptionKey: 'landing.pricing.starter.description',
    featureKeys: [
      'landing.pricing.starter.feature1',
      'landing.pricing.starter.feature2',
      'landing.pricing.starter.feature3',
      'landing.pricing.starter.feature4',
      'landing.pricing.starter.feature5',
    ],
    ctaKey: 'landing.pricing.starter.cta',
  },
  {
    nameKey: 'landing.pricing.pro.name',
    price: '$24,800',
    descriptionKey: 'landing.pricing.pro.description',
    featureKeys: [
      'landing.pricing.pro.feature1',
      'landing.pricing.pro.feature2',
      'landing.pricing.pro.feature3',
      'landing.pricing.pro.feature4',
      'landing.pricing.pro.feature5',
    ],
    ctaKey: 'landing.pricing.pro.cta',
    featured: true,
  },
  {
    nameKey: 'landing.pricing.sovereign.name',
    price: '$52,000',
    descriptionKey: 'landing.pricing.sovereign.description',
    featureKeys: [
      'landing.pricing.sovereign.feature1',
      'landing.pricing.sovereign.feature2',
      'landing.pricing.sovereign.feature3',
      'landing.pricing.sovereign.feature4',
      'landing.pricing.sovereign.feature5',
    ],
    ctaKey: 'landing.pricing.sovereign.cta',
  },
];

const currencyFormatter = new Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
  maximumFractionDigits: 0,
});

const DEFAULT_HEADCOUNT = 1_250;
const DEFAULT_REVENUE = 120_000_000;
const BASE_ANNUAL_SAVINGS = 384_000;
const IMPLEMENTATION_INVESTMENT = 102_400;

function calculateSavings(headcount: number, revenue: number): number {
  const headcountFactor = headcount / DEFAULT_HEADCOUNT;
  const revenueFactor = revenue / DEFAULT_REVENUE;
  const blended = 0.4 * headcountFactor + 0.6 * revenueFactor;

  return Math.round((BASE_ANNUAL_SAVINGS * blended) / 1_000) * 1_000;
}

function formatCompactUsd(value: number): string {
  if (value >= 1_000_000_000) {
    return `$${(value / 1_000_000_000).toFixed(1)}B`;
  }

  if (value >= 1_000_000) {
    return `$${(value / 1_000_000).toFixed(1)}M`;
  }

  return currencyFormatter.format(value);
}

export function PricingRoiSection() {
  const { t, intlLocale } = useI18n();
  const [headcount, setHeadcount] = useState(DEFAULT_HEADCOUNT);
  const [revenue, setRevenue] = useState(DEFAULT_REVENUE);

  const annualSavings = calculateSavings(headcount, revenue);
  const monthlySavings = annualSavings / 12;
  const paybackMonths =
    monthlySavings > 0 ? (IMPLEMENTATION_INVESTMENT / monthlySavings).toFixed(1) : '0.0';

  return (
    <section
      id="pricing"
      className="relative overflow-hidden border-y border-line bg-surface-muted"
    >
      <div
        className="bg-grid-light pointer-events-none absolute inset-0 opacity-50"
        aria-hidden="true"
      />
      <div className="landing-shell relative py-20 sm:py-24">
        <div className="mx-auto max-w-3xl text-center">
          <span className="stitch-pill border-cyan/30 bg-cyan/10 text-charcoal-strong">
            <Sparkles className="h-3.5 w-3.5 text-cyan-deep" aria-hidden="true" />
            {t('landing.pricing.eyebrow')}
          </span>
          <h2 className="mt-5 text-balance text-3xl font-bold tracking-[-0.04em] text-charcoal-strong sm:text-4xl">
            {t('landing.pricing.title')}
          </h2>
          <p className="mt-4 text-base leading-7 text-charcoal-light">
            {t('landing.pricing.description')}
          </p>
        </div>

        <div className="mt-12 grid gap-5 lg:grid-cols-3">
          {TIERS.map((tier) => (
            <article
              key={tier.nameKey}
              className={cn(
                'relative flex flex-col rounded-2xl border bg-surface p-6 shadow-sm transition-transform duration-300 hover:-translate-y-1',
                tier.featured
                  ? 'glow-cyan border-cyan/60 ring-1 ring-cyan/20'
                  : 'border-line hover:shadow-md',
              )}
            >
              {tier.featured ? (
                <span className="absolute -top-3 left-1/2 -translate-x-1/2 rounded-full bg-sidebar px-3 py-1 text-[10px] font-bold uppercase tracking-[0.16em] text-mint-accent">
                  {t('landing.pricing.featured')}
                </span>
              ) : null}
              <p className="text-2xs font-bold uppercase tracking-[0.16em] text-charcoal-light">
                {t(tier.nameKey)}
              </p>
              <div className="mt-3 flex items-end gap-1">
                <span className="text-4xl font-bold tracking-[-0.05em] text-charcoal-strong">
                  {tier.price}
                </span>
                <span className="pb-1 text-sm text-charcoal-light">
                  {t('landing.pricing.cadence')}
                </span>
              </div>
              <p className="mt-3 min-h-12 text-sm leading-6 text-charcoal-light">
                {t(tier.descriptionKey)}
              </p>
              <ul className="mt-5 space-y-2.5 border-t border-line pt-5 text-sm">
                {tier.featureKeys.map((featureKey) => (
                  <li key={featureKey} className="flex items-start gap-2 text-charcoal">
                    <CheckCircle2
                      className="mt-0.5 h-4 w-4 shrink-0 text-cyan-deep"
                      aria-hidden="true"
                    />
                    {t(featureKey)}
                  </li>
                ))}
              </ul>
              <button
                type="button"
                className={cn(
                  'mt-6 inline-flex h-11 w-full items-center justify-center gap-2 rounded-xl text-sm font-semibold transition-colors duration-300',
                  tier.featured
                    ? 'bg-cyan text-on-accent hover:bg-cyan-deep hover:text-white'
                    : 'border border-line bg-surface text-charcoal-strong hover:border-cyan/50 hover:bg-cyan/5',
                )}
              >
                {t(tier.ctaKey)}
                <ArrowRight className="h-4 w-4" aria-hidden="true" />
              </button>
            </article>
          ))}
        </div>

        <div className="mt-10 overflow-hidden rounded-2xl border border-line bg-surface shadow-sm">
          <div className="grid gap-0 lg:grid-cols-[1.2fr_0.8fr]">
            <div className="p-6 sm:p-8">
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div>
                  <p className="text-2xs font-bold uppercase tracking-[0.18em] text-charcoal-light">
                    {t('landing.roi.eyebrow')}
                  </p>
                  <h3 className="mt-1 text-xl font-bold text-charcoal-strong">
                    {t('landing.roi.title')}
                  </h3>
                </div>
                <span className="inline-flex items-center gap-1.5 rounded-full bg-mint-accent/40 px-3 py-1 text-xs font-semibold text-on-accent">
                  <Gauge className="h-3.5 w-3.5" aria-hidden="true" />
                  {t('landing.roi.live')}
                </span>
              </div>

              <div className="mt-8 grid gap-8 sm:grid-cols-2">
                <label className="block">
                  <span className="flex items-center justify-between text-sm font-medium text-charcoal">
                    <span className="inline-flex items-center gap-2">
                      <Users className="h-4 w-4 text-cyan-deep" aria-hidden="true" />
                      {t('landing.roi.headcount')}
                    </span>
                    <span className="rounded-md bg-surface-muted px-2 py-1 font-mono text-xs text-charcoal-strong">
                      {headcount.toLocaleString(intlLocale)}
                    </span>
                  </span>
                  <input
                    type="range"
                    min={100}
                    max={10_000}
                    step={50}
                    value={headcount}
                    onChange={(event) => setHeadcount(Number(event.target.value))}
                    className="mt-4 h-2 w-full cursor-pointer appearance-none rounded-full bg-line accent-teal-primary"
                    aria-label={t('landing.roi.headcountAria')}
                  />
                  <span className="mt-2 flex justify-between text-[10px] font-medium uppercase tracking-wider text-charcoal-light">
                    <span>100</span>
                    <span>{(10_000).toLocaleString(intlLocale)}</span>
                  </span>
                </label>

                <label className="block">
                  <span className="flex items-center justify-between text-sm font-medium text-charcoal">
                    <span className="inline-flex items-center gap-2">
                      <Wallet className="h-4 w-4 text-cyan-deep" aria-hidden="true" />
                      {t('landing.roi.revenue')}
                    </span>
                    <span className="rounded-md bg-surface-muted px-2 py-1 font-mono text-xs text-charcoal-strong">
                      {formatCompactUsd(revenue)}
                    </span>
                  </span>
                  <input
                    type="range"
                    min={5_000_000}
                    max={500_000_000}
                    step={5_000_000}
                    value={revenue}
                    onChange={(event) => setRevenue(Number(event.target.value))}
                    className="mt-4 h-2 w-full cursor-pointer appearance-none rounded-full bg-line accent-teal-primary"
                    aria-label={t('landing.roi.revenueAria')}
                  />
                  <span className="mt-2 flex justify-between text-[10px] font-medium uppercase tracking-wider text-charcoal-light">
                    <span>$5M</span>
                    <span>$500M</span>
                  </span>
                </label>
              </div>

              <div className="mt-8 grid gap-3 sm:grid-cols-3">
                <div className="rounded-xl border border-line bg-surface-muted p-4">
                  <p className="text-2xs font-bold uppercase tracking-[0.14em] text-charcoal-light">
                    {t('landing.roi.annual')}
                  </p>
                  <p className="mt-2 text-2xl font-bold tracking-tight text-charcoal-strong">
                    {currencyFormatter.format(annualSavings)}
                    <span className="text-sm font-medium text-charcoal-light">
                      {t('landing.roi.annualCadence')}
                    </span>
                  </p>
                </div>
                <div className="rounded-xl border border-line bg-surface-muted p-4">
                  <p className="text-2xs font-bold uppercase tracking-[0.14em] text-charcoal-light">
                    {t('landing.roi.monthly')}
                  </p>
                  <p className="mt-2 text-2xl font-bold tracking-tight text-charcoal-strong">
                    {currencyFormatter.format(Math.round(monthlySavings))}
                    <span className="text-sm font-medium text-charcoal-light">
                      {t('landing.roi.monthlyCadence')}
                    </span>
                  </p>
                </div>
                <div className="rounded-xl border border-cyan/40 bg-cyan/10 p-4">
                  <p className="text-2xs font-bold uppercase tracking-[0.14em] text-charcoal-light">
                    {t('landing.roi.payback')}
                  </p>
                  <p className="mt-2 text-2xl font-bold tracking-tight text-charcoal-strong">
                    {paybackMonths}
                    <span className="text-sm font-medium text-charcoal-light">
                      {' '}
                      {t('landing.roi.months')}
                    </span>
                  </p>
                </div>
              </div>
            </div>

            <div className="flex flex-col justify-between bg-forest p-6 text-white sm:p-8">
              <div>
                <p className="inline-flex items-center gap-2 rounded-full border border-white/15 bg-white/5 px-3 py-1 text-[10px] font-bold uppercase tracking-[0.16em] text-mint-accent">
                  <TrendingUp className="h-3.5 w-3.5" aria-hidden="true" />
                  {t('landing.roi.paybackAside')}
                </p>
                <p className="mt-5 text-3xl font-bold tracking-[-0.04em]">
                  {paybackMonths} {t('landing.roi.months')}
                </p>
                <p className="mt-2 text-sm leading-6 text-sage-light/80">
                  {t('landing.roi.asideBody')}
                </p>
              </div>
              <div className="mt-8 space-y-3 border-t border-white/10 pt-5 text-xs text-sage-light/75">
                <p className="flex items-center justify-between gap-4">
                  <span>{t('landing.roi.consolidated')}</span>
                  <span className="font-mono text-white">{t('landing.roi.consolidatedValue')}</span>
                </p>
                <p className="flex items-center justify-between gap-4">
                  <span>{t('landing.roi.reconciled')}</span>
                  <span className="font-mono text-white">{t('landing.roi.reconciledValue')}</span>
                </p>
                <p className="flex items-center justify-between gap-4">
                  <span>{t('landing.roi.projection')}</span>
                  <span className="font-mono text-mint-accent">
                    {t('landing.roi.projectionValue')}
                  </span>
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
