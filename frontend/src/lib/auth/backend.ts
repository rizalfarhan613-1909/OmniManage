import { getApiBaseUrl, getBackendTimeoutMs } from '@/lib/config/env';
import { t } from '@/lib/i18n';

import type { ApiResponse } from '@/lib/api/types';
import type { Session } from './types';

export interface BackendLoginPayload {
  accessToken: string;
  tokenType: string;
  expiresAt: string;
  session: Session;
  refreshToken: string;
  refreshTokenExpiresAt: string;
}

export interface BackendResponse<T> {
  status: number;
  envelope: ApiResponse<T>;
}

interface LoginRequestBody {
  tenantCode: string;
  email: string;
  password: string;
}

function traceId(): string {
  return crypto.randomUUID();
}

function meta() {
  return { traceId: traceId(), timestamp: new Date().toISOString() };
}

function unavailable<T>(): BackendResponse<T> {
  return {
    status: 503,
    envelope: {
      success: false,
      data: null,
      error: { code: 'SYSTEM.UNAVAILABLE', message: t('errors.network') },
      meta: meta(),
    },
  };
}

async function readJson<T>(response: Response): Promise<ApiResponse<T> | null> {
  try {
    const payload: unknown = await response.json();

    if (
      typeof payload === 'object' &&
      payload !== null &&
      typeof (payload as { success?: unknown }).success === 'boolean'
    ) {
      return payload as ApiResponse<T>;
    }
  } catch {
    // Fall through: the caller turns a malformed body into a 503 envelope.
  }

  return null;
}

async function callBackend<T>(path: string, init: RequestInit): Promise<BackendResponse<T>> {
  const headers = new Headers(init.headers);
  headers.set('Accept', 'application/json');

  try {
    const response = await fetch(`${getApiBaseUrl()}${path}`, {
      ...init,
      cache: 'no-store',
      signal: AbortSignal.timeout(getBackendTimeoutMs()),
      headers,
    });

    const envelope = await readJson<T>(response);

    if (envelope) {
      return { status: response.status, envelope };
    }

    return unavailable<T>();
  } catch {
    return unavailable<T>();
  }
}

export function backendLogin(
  payload: LoginRequestBody,
  acceptLanguage = 'en-US,en;q=0.9,id;q=0.8',
): Promise<BackendResponse<BackendLoginPayload>> {
  return callBackend<BackendLoginPayload>('/api/v1/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', 'Accept-Language': acceptLanguage },
    body: JSON.stringify(payload),
  });
}

export function backendRefresh(
  refreshToken: string,
): Promise<BackendResponse<BackendLoginPayload>> {
  return callBackend<BackendLoginPayload>('/api/v1/auth/refresh', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken }),
  });
}

export function backendLogout(
  refreshToken: string,
): Promise<BackendResponse<{ loggedOut: boolean }>> {
  return callBackend<{ loggedOut: boolean }>('/api/v1/auth/logout', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken }),
  });
}

export function backendSession(accessToken: string): Promise<BackendResponse<Session>> {
  return callBackend<Session>('/api/v1/auth/session', {
    method: 'GET',
    headers: { Authorization: `Bearer ${accessToken}` },
  });
}

/**
 * Generic authenticated proxy call used by `/api/proxy/*`.
 * `segments` are individual path segments and are encoded here.
 */
export function backendRequest(
  segments: readonly string[],
  init: {
    method: string;
    search: string;
    headers: HeadersInit;
    body?: ArrayBuffer;
    accessToken: string;
  },
): Promise<BackendResponse<unknown>> {
  const path = segments.map((segment) => encodeURIComponent(segment)).join('/');

  return callBackend<unknown>(`/api/v1/${path}${init.search}`, {
    method: init.method,
    headers: {
      ...init.headers,
      Authorization: `Bearer ${init.accessToken}`,
    },
    body: init.body,
  });
}
