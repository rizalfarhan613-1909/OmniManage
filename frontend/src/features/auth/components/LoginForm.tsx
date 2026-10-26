'use client';

import { zodResolver } from '@hookform/resolvers/zod';
import { Eye, EyeOff, LogIn } from 'lucide-react';
import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { useForm } from 'react-hook-form';

import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { loginRequestSchema, type LoginRequest } from '@/features/auth/schemas/login.schema';
import { apiClient } from '@/lib/api/client';
import { ApiRequestError } from '@/lib/api/errors';
import { BFF_ROUTES } from '@/lib/api/routes';
import type { Session } from '@/lib/auth/types';
import { t } from '@/lib/i18n';

export function LoginForm() {
  const router = useRouter();
  const [isPasswordVisible, setIsPasswordVisible] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    setError,
  } = useForm<LoginRequest>({
    resolver: zodResolver(loginRequestSchema),
    defaultValues: { tenantCode: '', email: '', password: '' },
    mode: 'onBlur',
  });

  const onSubmit = handleSubmit(async (values) => {
    setFormError(null);

    try {
      // The BFF stores JWTs in httpOnly cookies; only the session payload is
      // returned to this component (Security.md §2.5).
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
          className="border-danger/40 bg-danger/10 rounded-md border px-3 py-2 text-sm text-danger"
        >
          {formError}
        </div>
      ) : null}

      <div>
        <label htmlFor="tenantCode" className="mb-1 block text-sm font-medium text-charcoal">
          {t('login.tenantCode')}
        </label>
        <Input
          id="tenantCode"
          autoComplete="organization"
          placeholder={t('login.tenantCodePlaceholder')}
          hasError={Boolean(errors.tenantCode)}
          aria-describedby={errors.tenantCode ? 'tenantCode-error' : undefined}
          {...register('tenantCode')}
        />
        {errors.tenantCode ? (
          <p id="tenantCode-error" className="mt-1 text-xs text-danger">
            {errors.tenantCode.message}
          </p>
        ) : null}
      </div>

      <div>
        <label htmlFor="email" className="mb-1 block text-sm font-medium text-charcoal">
          {t('login.email')}
        </label>
        <Input
          id="email"
          type="email"
          inputMode="email"
          autoComplete="username"
          placeholder={t('login.emailPlaceholder')}
          hasError={Boolean(errors.email)}
          aria-describedby={errors.email ? 'email-error' : undefined}
          {...register('email')}
        />
        {errors.email ? (
          <p id="email-error" className="mt-1 text-xs text-danger">
            {errors.email.message}
          </p>
        ) : null}
      </div>

      <div>
        <label htmlFor="password" className="mb-1 block text-sm font-medium text-charcoal">
          {t('login.password')}
        </label>
        <div className="relative">
          <Input
            id="password"
            type={isPasswordVisible ? 'text' : 'password'}
            autoComplete="current-password"
            placeholder={t('login.passwordPlaceholder')}
            hasError={Boolean(errors.password)}
            aria-describedby={errors.password ? 'password-error' : undefined}
            className="pr-11"
            {...register('password')}
          />
          <button
            type="button"
            onClick={() => setIsPasswordVisible((visible) => !visible)}
            aria-label={isPasswordVisible ? t('login.hidePassword') : t('login.showPassword')}
            aria-pressed={isPasswordVisible}
            className="absolute inset-y-0 right-0 flex w-10 items-center justify-center rounded-r-md text-charcoal-light hover:text-charcoal focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary"
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

      <Button type="submit" className="w-full" disabled={isSubmitting} aria-busy={isSubmitting}>
        <LogIn className="h-4 w-4" aria-hidden="true" />
        {isSubmitting ? t('login.submitting') : t('login.submit')}
      </Button>
    </form>
  );
}
