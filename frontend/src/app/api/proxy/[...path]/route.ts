import { NextResponse, type NextRequest } from 'next/server';

import { backendRefresh, backendRequest } from '@/lib/auth/backend';
import {
  ACCESS_TOKEN_COOKIE,
  REFRESH_TOKEN_COOKIE,
  clearSessionCookies,
  setSessionCookies,
} from '@/lib/auth/cookies';
import { assertSameOrigin, envelopeError, unauthenticated } from '@/lib/api/bff';
import { API_ERROR_CODES } from '@/lib/api/errors';
import type { ApiResponse } from '@/lib/api/types';
import type { BackendLoginPayload } from '@/lib/auth/backend';
import { t } from '@/lib/i18n';

interface RouteContext {
  params: Promise<{ path: string[] }>;
}

const MUTATING_METHODS = new Set(['POST', 'PUT', 'PATCH', 'DELETE']);
const FORWARDED_HEADERS = [
  'content-type',
  'accept',
  'accept-language',
  'idempotency-key',
  'if-match',
  'x-device-id',
] as const;

function forwardHeaders(request: NextRequest): HeadersInit {
  const headers: Record<string, string> = {};

  for (const name of FORWARDED_HEADERS) {
    const value = request.headers.get(name);

    if (value) {
      headers[name] = value;
    }
  }

  return headers;
}

function hasUnsafeSegment(segments: readonly string[]): boolean {
  return segments.some(
    (segment) =>
      segment.length === 0 || segment === '.' || segment === '..' || /[/\\]/.test(segment),
  );
}

async function handle(request: NextRequest, context: RouteContext): Promise<NextResponse> {
  if (MUTATING_METHODS.has(request.method)) {
    const csrfError = assertSameOrigin(request);

    if (csrfError) {
      return csrfError;
    }
  }

  const { path } = await context.params;

  // Authentication flows are only reachable through the dedicated BFF routes,
  // which keep tokens out of JavaScript-readable responses.
  if (path[0] === 'auth') {
    return envelopeError(API_ERROR_CODES.forbidden, t('states.forbidden.body'), 403);
  }

  if (path.length === 0 || hasUnsafeSegment(path)) {
    return envelopeError(API_ERROR_CODES.unexpected, t('errors.notFound.body'), 404);
  }

  let accessToken = request.cookies.get(ACCESS_TOKEN_COOKIE)?.value;
  const refreshToken = request.cookies.get(REFRESH_TOKEN_COOKIE)?.value;
  let refreshed: BackendLoginPayload | null = null;

  if (!accessToken && refreshToken) {
    const rotation = await backendRefresh(refreshToken);

    if (!rotation.envelope.success) {
      const response = unauthenticated();
      clearSessionCookies(response);
      return response;
    }

    refreshed = rotation.envelope.data;
    accessToken = refreshed.accessToken;
  }

  if (!accessToken) {
    return unauthenticated();
  }

  const method = request.method.toUpperCase();
  const body = method === 'GET' || method === 'HEAD' ? undefined : await request.arrayBuffer();

  const send = (token: string) =>
    backendRequest(path, {
      method,
      search: request.nextUrl.search,
      headers: forwardHeaders(request),
      body: body && body.byteLength > 0 ? body : undefined,
      accessToken: token,
    });

  let result = await send(accessToken);

  if (result.status === 401 && refreshToken) {
    const rotation = await backendRefresh(refreshToken);

    if (rotation.envelope.success) {
      refreshed = rotation.envelope.data;
      accessToken = refreshed.accessToken;
      result = await send(accessToken);
    }
  }

  const response = NextResponse.json<ApiResponse<unknown>>(result.envelope, {
    status: result.status,
    headers: {
      'x-trace-id': result.envelope.meta.traceId,
      ...(refreshed ? { 'x-session-refreshed': '1' } : {}),
    },
  });

  if (refreshed) {
    setSessionCookies(response, {
      accessToken: refreshed.accessToken,
      accessTokenExpiresAt: refreshed.expiresAt,
      refreshToken: refreshed.refreshToken,
      refreshTokenExpiresAt: refreshed.refreshTokenExpiresAt,
    });
  }

  if (result.status === 401) {
    clearSessionCookies(response);
  }

  return response;
}

export function GET(request: NextRequest, context: RouteContext) {
  return handle(request, context);
}

export function POST(request: NextRequest, context: RouteContext) {
  return handle(request, context);
}

export function PUT(request: NextRequest, context: RouteContext) {
  return handle(request, context);
}

export function PATCH(request: NextRequest, context: RouteContext) {
  return handle(request, context);
}

export function DELETE(request: NextRequest, context: RouteContext) {
  return handle(request, context);
}

export function HEAD(request: NextRequest, context: RouteContext) {
  return handle(request, context);
}
