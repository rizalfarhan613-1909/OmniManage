import { NextResponse, type NextRequest } from 'next/server';
import type { ZodError } from 'zod';

import { t } from '@/lib/i18n';

import { API_ERROR_CODES } from './errors';
import type { ApiErrorDetail, ApiMeta, ApiResponse } from './types';

export function bffMeta(): ApiMeta {
  return { traceId: crypto.randomUUID(), timestamp: new Date().toISOString() };
}

export function envelopeOk<T>(data: T, status = 200): NextResponse<ApiResponse<T>> {
  const meta = bffMeta();

  return NextResponse.json<ApiResponse<T>>(
    { success: true, data, error: null, meta },
    { status, headers: { 'x-trace-id': meta.traceId } },
  );
}

export function envelopeError(
  code: string,
  message: string,
  status: number,
  details?: ApiErrorDetail[],
): NextResponse<ApiResponse<never>> {
  const meta = bffMeta();

  return NextResponse.json<ApiResponse<never>>(
    {
      success: false,
      data: null,
      error: details && details.length > 0 ? { code, message, details } : { code, message },
      meta,
    },
    { status, headers: { 'x-trace-id': meta.traceId } },
  );
}

export function unauthenticated(): NextResponse<ApiResponse<never>> {
  return envelopeError(API_ERROR_CODES.unauthenticated, t('login.sessionExpired'), 401);
}

/**
 * SameSite=Lax cookies plus an Origin check on mutating requests — the CSRF
 * strategy documented in Security.md §2.5 / §12.
 */
export function assertSameOrigin(request: NextRequest): NextResponse<ApiResponse<never>> | null {
  const origin = request.headers.get('origin');

  if (!origin) {
    // Same-origin navigations and non-browser clients do not send Origin.
    // SameSite=Lax still protects cookie-bearing cross-site requests.
    return null;
  }

  try {
    if (new URL(origin).host === request.headers.get('host')) {
      return null;
    }
  } catch {
    // Invalid Origin header — reject below.
  }

  return envelopeError(API_ERROR_CODES.forbidden, t('states.forbidden.body'), 403);
}

export function validationError(error: ZodError): NextResponse<ApiResponse<never>> {
  const details: ApiErrorDetail[] = error.issues.map((issue) => ({
    field: issue.path.join('.') || undefined,
    code: issue.code.toUpperCase(),
    message: issue.message,
  }));

  return envelopeError(API_ERROR_CODES.validationFailed, t('validation.summary'), 400, details);
}
