import { NextResponse, type NextRequest } from 'next/server';

import { backendRefresh, backendSession } from '@/lib/auth/backend';
import {
  ACCESS_TOKEN_COOKIE,
  REFRESH_TOKEN_COOKIE,
  clearSessionCookies,
  setSessionCookies,
} from '@/lib/auth/cookies';
import { unauthenticated } from '@/lib/api/bff';
import type { ApiResponse } from '@/lib/api/types';
import type { Session } from '@/lib/auth/types';

/**
 * Session re-validation used by the shell on app focus and every 5 minutes
 * (DESIGN.md §3.2). If the access token has expired the route transparently
 * refreshes it and rotates the cookies.
 */
export async function GET(request: NextRequest): Promise<NextResponse> {
  const accessToken = request.cookies.get(ACCESS_TOKEN_COOKIE)?.value;
  const refreshToken = request.cookies.get(REFRESH_TOKEN_COOKIE)?.value;

  if (!accessToken && !refreshToken) {
    return unauthenticated();
  }

  if (accessToken) {
    const result = await backendSession(accessToken);

    if (result.envelope.success) {
      return NextResponse.json<ApiResponse<Session>>(result.envelope, {
        status: result.status,
        headers: { 'x-trace-id': result.envelope.meta.traceId },
      });
    }

    if (result.status !== 401) {
      return NextResponse.json<ApiResponse<never>>(result.envelope, {
        status: result.status,
        headers: { 'x-trace-id': result.envelope.meta.traceId },
      });
    }
  }

  if (!refreshToken) {
    return unauthenticated();
  }

  const refreshed = await backendRefresh(refreshToken);

  if (!refreshed.envelope.success) {
    const response = NextResponse.json<ApiResponse<never>>(refreshed.envelope, {
      status: refreshed.status,
      headers: { 'x-trace-id': refreshed.envelope.meta.traceId },
    });

    if (refreshed.status === 401 || refreshed.status === 403) {
      clearSessionCookies(response);
    }

    return response;
  }

  const {
    accessToken: nextAccessToken,
    expiresAt,
    refreshToken: rotated,
    refreshTokenExpiresAt,
  } = refreshed.envelope.data;

  const sessionResult = await backendSession(nextAccessToken);

  if (!sessionResult.envelope.success) {
    return NextResponse.json<ApiResponse<never>>(sessionResult.envelope, {
      status: sessionResult.status,
      headers: { 'x-trace-id': sessionResult.envelope.meta.traceId },
    });
  }

  const response = NextResponse.json<ApiResponse<Session>>(sessionResult.envelope, {
    status: sessionResult.status,
    headers: { 'x-trace-id': sessionResult.envelope.meta.traceId },
  });

  setSessionCookies(response, {
    accessToken: nextAccessToken,
    accessTokenExpiresAt: expiresAt,
    refreshToken: rotated,
    refreshTokenExpiresAt,
  });

  return response;
}
