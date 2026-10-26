import { NextResponse, type NextRequest } from 'next/server';

import { backendLogin } from '@/lib/auth/backend';
import { setSessionCookies } from '@/lib/auth/cookies';
import { assertSameOrigin, envelopeError, validationError } from '@/lib/api/bff';
import { API_ERROR_CODES } from '@/lib/api/errors';
import type { ApiResponse } from '@/lib/api/types';
import type { Session } from '@/lib/auth/types';
import { loginRequestSchema } from '@/features/auth/schemas/login.schema';
import { t } from '@/lib/i18n';

/**
 * BFF login: exchanges credentials with the backend, stores JWTs in httpOnly
 * cookies and returns only the sanitized session payload to the browser.
 */
export async function POST(request: NextRequest): Promise<NextResponse> {
  const csrfError = assertSameOrigin(request);

  if (csrfError) {
    return csrfError;
  }

  let payload: unknown;

  try {
    payload = await request.json();
  } catch {
    return envelopeError(API_ERROR_CODES.validationFailed, t('errors.unexpected'), 400);
  }

  const parsed = loginRequestSchema.safeParse(payload);

  if (!parsed.success) {
    return validationError(parsed.error);
  }

  const result = await backendLogin(parsed.data);

  if (!result.envelope.success) {
    return NextResponse.json<ApiResponse<never>>(result.envelope, {
      status: result.status,
      headers: { 'x-trace-id': result.envelope.meta.traceId },
    });
  }

  const { accessToken, expiresAt, refreshToken, refreshTokenExpiresAt, session } =
    result.envelope.data;

  const response = NextResponse.json<ApiResponse<Session>>(
    {
      success: true,
      data: session,
      error: null,
      meta: result.envelope.meta,
    },
    { status: 200, headers: { 'x-trace-id': result.envelope.meta.traceId } },
  );

  setSessionCookies(response, {
    accessToken,
    accessTokenExpiresAt: expiresAt,
    refreshToken,
    refreshTokenExpiresAt,
  });

  return response;
}
