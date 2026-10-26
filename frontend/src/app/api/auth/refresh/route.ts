import { NextResponse, type NextRequest } from 'next/server';

import { backendRefresh } from '@/lib/auth/backend';
import { REFRESH_TOKEN_COOKIE, clearSessionCookies, setSessionCookies } from '@/lib/auth/cookies';
import { assertSameOrigin, unauthenticated } from '@/lib/api/bff';
import type { ApiResponse } from '@/lib/api/types';
import type { Session } from '@/lib/auth/types';

/**
 * Silent session refresh. Rotates both cookies and returns the refreshed
 * session payload; tokens never appear in the response body.
 */
export async function POST(request: NextRequest): Promise<NextResponse> {
  const csrfError = assertSameOrigin(request);

  if (csrfError) {
    return csrfError;
  }

  const refreshToken = request.cookies.get(REFRESH_TOKEN_COOKIE)?.value;

  if (!refreshToken) {
    return unauthenticated();
  }

  const result = await backendRefresh(refreshToken);

  if (!result.envelope.success) {
    const response = NextResponse.json<ApiResponse<never>>(result.envelope, {
      status: result.status,
      headers: { 'x-trace-id': result.envelope.meta.traceId },
    });

    if (result.status === 401 || result.status === 403) {
      clearSessionCookies(response);
    }

    return response;
  }

  const {
    accessToken,
    expiresAt,
    refreshToken: rotated,
    refreshTokenExpiresAt,
    session,
  } = result.envelope.data;

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
    refreshToken: rotated,
    refreshTokenExpiresAt,
  });

  return response;
}
