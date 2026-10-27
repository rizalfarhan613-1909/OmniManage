'use client';

import { zodResolver } from '@hookform/resolvers/zod';
import {
  ArrowRight,
  Building2,
  Check,
  Crown,
  Eye,
  EyeOff,
  Factory,
  Fingerprint,
  Globe2,
  KeyRound,
  Mail,
  ShieldCheck,
  Sparkles,
  Users,
  type LucideIcon,
} from 'lucide-react';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { useForm, useWatch } from 'react-hook-form';

import { useI18n } from '@/components/providers/LanguageProvider';
import { loginRequestSchema, type LoginRequest } from '@/features/auth/schemas/login.schema';
import { apiClient } from '@/lib/api/client';
import { ApiRequestError } from '@/lib/api/errors';
import { BFF_ROUTES } from '@/lib/api/routes';
import type { Session } from '@/lib/auth/types';
import type { TranslationKey } from '@/lib/i18n';
import { cn } from '@/lib/utils/cn';

type Persona = {
  id: string;
  labelKey: TranslationKey;
  detailKey: TranslationKey;
  tenantCode: string;
  email: string;
  password: string;
  icon: LucideIcon;
};

const PERSONAS: readonly Persona[] = [
  {
    id: 'ceo',
    labelKey: 'login.persona.ceo.label',
    detailKey: 'login.persona.ceo.detail',
    tenantCode: 'acme-global',
    email: 'sarah.jenkins@acmeglobal.com',
    password: 'Demo!Passw0rd',
    icon: Crown,
  },
  {
    id: 'superadmin',
    labelKey: 'login.persona.superadmin.label',
    detailKey: 'login.persona.superadmin.detail',
    tenantCode: 'platform-root',
    email: 'root@omnimanage.io',
    password: 'Demo!Passw0rd',
    icon: ShieldCheck,
  },
  {
    id: 'hr',
    labelKey: 'login.persona.hr.label',
    detailKey: 'login.persona.hr.detail',
    tenantCode: 'acme-global',
    email: 'elena.rostova@acmeglobal.com',
    password: 'Demo!Passw0rd',
    icon: Users,
  },
  {
    id: 'plant',
    labelKey: 'login.persona.plant.label',
    detailKey: 'login.persona.plant.detail',
    tenantCode: 'acme-global',
    email: 'klaus.weber@acmeglobal.com',
    password: 'Demo!Passw0rd',
    icon: Factory,
  },
];

const COMPLIANCE_PILLS: readonly { labelKey: TranslationKey; icon: LucideIcon }[] = [
  { labelKey: 'login.compliance.soc', icon: ShieldCheck },
  { labelKey: 'login.compliance.hipaa', icon: Fingerprint },
  { labelKey: 'login.compliance.tls', icon: KeyRound },
];

function slugifyTenant(value: string): string {
  const slug = value
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '');

  return slug || 'acme-global';
}

export function LoginForm() {
  const router = useRouter();
  const { t } = useI18n();
  const [isPasswordVisible, setIsPasswordVisible] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [activePersona, setActivePersona] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    control,
    setValue,
    formState: { errors, isSubmitting },
    setError,
  } = useForm<LoginRequest>({
    resolver: zodResolver(loginRequestSchema),
    defaultValues: {
      tenantCode: 'acme-global',
      email: 'sarah.jenkins@acmeglobal.com',
      password: '',
    },
    mode: 'onBlur',
  });

  const tenantCode = useWatch({ control, name: 'tenantCode' });
  const workspaceUrl = `https://${slugifyTenant(tenantCode ?? '')}.omnimanage.io`;

  const applyPersona = (persona: Persona) => {
    setActivePersona(persona.id);
    setValue('tenantCode', persona.tenantCode, { shouldValidate: true, shouldDirty: true });
    setValue('email', persona.email, { shouldValidate: true, shouldDirty: true });
    setValue('password', persona.password, { shouldValidate: true, shouldDirty: true });
    setFormError(null);
  };

  const onSubmit = handleSubmit(async (values) => {
    setFormError(null);

    try {
      // The BFF stores JWTs in httpOnly cookies; only the session payload is
      // returned to this component (Security.md 2.5).
      await apiClient.post<Session>(BFF_ROUTES.login, values, {
        skipAuthRefresh: true,
      });

      router.replace('/dashboard');
      router.refresh();
    } catch (error) {
      if (error instanceof ApiRequestError) {
        if (error.isUnauthenticated()) {
          setFormError(t('login.invalidCredentials'));
          return;
        }

        if (error.details.length > 0) {
          for (const detail of error.details) {
            if (
              detail.field === 'tenantCode' ||
              detail.field === 'email' ||
              detail.field === 'password'
            ) {
              setError(detail.field, { type: 'server', message: detail.message });
            }
          }
        }

        setFormError(error.message);
        return;
      }

      setFormError(t('errors.unexpected'));
    }
  });

  return (
    <form onSubmit={onSubmit} noValidate className="space-y-5">
      {formError ? (
        <div
          role="alert"
          className="rounded-xl border border-danger/30 bg-danger/10 px-3.5 py-2.5 text-sm text-danger"
        >
          {formError}
        </div>
      ) : null}

      <div>
        <div className="mb-1.5 flex items-center justify-between gap-3">
          <label htmlFor="tenantCode" className="text-sm font-semibold text-charcoal-strong">
            {t('login.tenantLabel')}
          </label>
          <span className="inline-flex items-center gap-1 text-[10px] font-semibold uppercase tracking-[0.12em] text-charcoal-strong">
            <Check className="h-3 w-3 text-cyan-deep" aria-hidden="true" />
            {t('login.verifiedRegistry')}
          </span>
        </div>
        <div className="relative">
          <Building2
            className="pointer-events-none absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-charcoal-light"
            aria-hidden="true"
          />
          <input
            id="tenantCode"
            autoComplete="organization"
            placeholder={t('login.tenantCodePlaceholder')}
            aria-invalid={errors.tenantCode ? true : undefined}
            aria-describedby={errors.tenantCode ? 'tenantCode-error' : undefined}
            className={cn(
              'h-11 w-full rounded-xl border bg-surface pl-10 pr-3 text-sm text-charcoal shadow-sm outline-none transition-colors placeholder:text-charcoal-light/70 focus:border-cyan focus:ring-2 focus:ring-cyan/25',
              errors.tenantCode ? 'border-danger' : 'border-line',
            )}
            {...register('tenantCode')}
          />
        </div>
        {errors.tenantCode ? (
          <p id="tenantCode-error" className="mt-1 text-xs text-danger">
            {errors.tenantCode.message}
          </p>
        ) : null}
        <div className="mt-2 flex items-center gap-2 rounded-lg border border-line bg-surface-muted px-3 py-2 text-[11px] text-charcoal-light">
          <Globe2 className="h-3.5 w-3.5 shrink-0 text-cyan-deep" aria-hidden="true" />
          <span className="shrink-0">{t('login.workspaceUrl')}</span>
          <span className="truncate font-mono font-semibold text-charcoal-strong">
            {workspaceUrl}
          </span>
        </div>
      </div>

      <div>
        <label htmlFor="email" className="mb-1.5 block text-sm font-semibold text-charcoal-strong">
          {t('login.emailLabel')}
        </label>
        <div className="relative">
          <Mail
            className="pointer-events-none absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-charcoal-light"
            aria-hidden="true"
          />
          <input
            id="email"
            type="email"
            inputMode="email"
            autoComplete="username"
            placeholder={t('login.emailPlaceholder')}
            aria-invalid={errors.email ? true : undefined}
            aria-describedby={errors.email ? 'email-error' : undefined}
            className={cn(
              'h-11 w-full rounded-xl border bg-surface pl-10 pr-3 text-sm text-charcoal shadow-sm outline-none transition-colors placeholder:text-charcoal-light/70 focus:border-cyan focus:ring-2 focus:ring-cyan/25',
              errors.email ? 'border-danger' : 'border-line',
            )}
            {...register('email')}
          />
        </div>
        {errors.email ? (
          <p id="email-error" className="mt-1 text-xs text-danger">
            {errors.email.message}
          </p>
        ) : null}
      </div>

      <div>
        <div className="mb-1.5 flex items-center justify-between gap-3">
          <label htmlFor="password" className="text-sm font-semibold text-charcoal-strong">
            {t('login.passwordLabel')}
          </label>
          <button
            type="button"
            className="text-xs font-semibold text-charcoal-strong underline-offset-2 transition-colors hover:text-cyan-deep hover:underline"
          >
            {t('login.forgot')}
          </button>
        </div>
        <div className="relative">
          <KeyRound
            className="pointer-events-none absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-charcoal-light"
            aria-hidden="true"
          />
          <input
            id="password"
            type={isPasswordVisible ? 'text' : 'password'}
            autoComplete="current-password"
            placeholder={t('login.passwordPlaceholderLong')}
            aria-invalid={errors.password ? true : undefined}
            aria-describedby={errors.password ? 'password-error' : undefined}
            className={cn(
              'h-11 w-full rounded-xl border bg-surface pl-10 pr-11 text-sm text-charcoal shadow-sm outline-none transition-colors placeholder:text-charcoal-light/70 focus:border-cyan focus:ring-2 focus:ring-cyan/25',
              errors.password ? 'border-danger' : 'border-line',
            )}
            {...register('password')}
          />
          <button
            type="button"
            onClick={() => setIsPasswordVisible((visible) => !visible)}
            aria-label={isPasswordVisible ? t('login.hidePassword') : t('login.showPassword')}
            aria-pressed={isPasswordVisible}
            className="absolute inset-y-0 right-0 flex w-11 items-center justify-center rounded-r-xl text-charcoal-light transition-colors hover:text-charcoal-strong focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary"
          >
            {isPasswordVisible ? (
              <EyeOff className="h-4 w-4" aria-hidden="true" />
            ) : (
              <Eye className="h-4 w-4" aria-hidden="true" />
            )}
          </button>
        </div>
        {errors.password ? (
          <p id="password-error" className="mt-1 text-xs text-danger">
            {errors.password.message}
          </p>
        ) : null}
      </div>

      <label className="flex cursor-pointer items-start gap-2.5 text-xs leading-5 text-charcoal-light">
        <input
          type="checkbox"
          defaultChecked
          className="mt-0.5 h-4 w-4 rounded border-line text-cyan accent-teal-primary"
        />
        <span>
          {t('login.remember')}{' '}
          <span className="text-charcoal-light/70">{t('login.rememberHint')}</span>
        </span>
      </label>

      <button
        type="submit"
        disabled={isSubmitting}
        aria-busy={isSubmitting}
        className="inline-flex h-12 w-full items-center justify-center gap-2 rounded-xl bg-cyan text-sm font-bold text-on-accent shadow-lg shadow-cyan/20 transition-all duration-300 hover:bg-cyan-deep hover:text-white disabled:cursor-not-allowed disabled:opacity-60"
      >
        {isSubmitting ? t('login.submitting') : t('login.submitLong')}
        <ArrowRight className="h-4 w-4" aria-hidden="true" />
      </button>

      <div className="flex items-center gap-3">
        <span className="h-px flex-1 bg-line" />
        <span className="text-[9px] font-bold uppercase tracking-[0.16em] text-charcoal-light">
          {t('login.ssoDivider')}
        </span>
        <span className="h-px flex-1 bg-line" />
      </div>

      <div className="grid gap-2 sm:grid-cols-2">
        <button
          type="button"
          className="inline-flex h-11 items-center justify-center gap-2 rounded-xl border border-line bg-surface text-sm font-semibold text-charcoal-strong transition-colors hover:border-cyan/50 hover:bg-cyan/5"
        >
          <Globe2 className="h-4 w-4 text-cyan-deep" aria-hidden="true" />
          {t('login.googleSso')}
        </button>
        <button
          type="button"
          className="inline-flex h-11 items-center justify-center gap-2 rounded-xl border border-line bg-surface text-sm font-semibold text-charcoal-strong transition-colors hover:border-cyan/50 hover:bg-cyan/5"
        >
          <ShieldCheck className="h-4 w-4 text-cyan-deep" aria-hidden="true" />
          {t('login.samlSso')}
        </button>
      </div>

      <div className="border-t border-line pt-5">
        <div className="flex items-center justify-between gap-3">
          <p className="inline-flex items-center gap-1.5 text-[10px] font-bold uppercase tracking-[0.14em] text-charcoal-light">
            <Sparkles className="h-3.5 w-3.5 text-cyan-deep" aria-hidden="true" />
            {t('login.personas.title')}
          </p>
          <span className="text-[10px] text-charcoal-light">{t('login.personas.hint')}</span>
        </div>

        <div className="mt-3 grid gap-2 sm:grid-cols-2">
          {PERSONAS.map((persona) => {
            const Icon = persona.icon;
            const isActive = activePersona === persona.id;

            return (
              <button
                key={persona.id}
                type="button"
                onClick={() => applyPersona(persona)}
                aria-label={t('login.personas.prefill', { persona: t(persona.labelKey) })}
                aria-pressed={isActive}
                className={cn(
                  'group flex items-start gap-2.5 rounded-xl border px-3 py-2.5 text-left transition-all duration-300',
                  isActive
                    ? 'border-cyan bg-cyan/10 shadow-sm'
                    : 'border-line bg-surface hover:border-cyan/50 hover:bg-cyan/5',
                )}
              >
                <span
                  className={cn(
                    'mt-0.5 flex h-7 w-7 shrink-0 items-center justify-center rounded-lg',
                    isActive ? 'bg-cyan text-on-accent' : 'bg-surface-muted text-charcoal-strong',
                  )}
                >
                  <Icon className="h-3.5 w-3.5" aria-hidden="true" />
                </span>
                <span className="min-w-0">
                  <span className="block truncate text-xs font-semibold text-charcoal-strong">
                    {t(persona.labelKey)}
                  </span>
                  <span className="block truncate text-[10px] text-charcoal-light">
                    {t(persona.detailKey)}
                  </span>
                </span>
              </button>
            );
          })}
        </div>
      </div>

      <div className="flex flex-wrap items-center justify-center gap-x-4 gap-y-2 border-t border-line pt-4">
        {COMPLIANCE_PILLS.map((pill) => {
          const Icon = pill.icon;

          return (
            <span
              key={pill.labelKey}
              className="inline-flex items-center gap-1.5 text-[10px] font-semibold text-charcoal-light"
            >
              <Icon className="h-3.5 w-3.5 text-cyan-deep" aria-hidden="true" />
              {t(pill.labelKey)}
            </span>
          );
        })}
      </div>
    </form>
  );
}
