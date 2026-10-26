import { t } from '@/lib/i18n';

import { API_ERROR_CODES, ApiRequestError } from './errors';
import { API_EVENTS, emitApiEvent } from './events';
import { BFF_ROUTES, resolveRequestPath } from './routes';
import type { ApiErrorDetail, ApiResponse, ApiSuccess, ApiMeta } from './types';

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

export interface ApiRequestOptions {
  method?: HttpMethod;
  body?: unknown;
  headers?: HeadersInit;
  signal?: AbortSignal;
  cache?: RequestCache;
  /**
   * Disable the silent refresh-and-retry cycle. Used by the login and refresh
   * calls themselves to avoid recursive refreshes.
   */
  skipAuthRefresh?: boolean;
}

interface RawEnvelope {
  envelope: ApiResponse<unknown>;
  response: Response;
}

let refreshPromise: Promise<boolean> | null = null;

function fallbackCodeForStatus(status: number): string {
  switch (status) {
    case 400:
      return API_ERROR_CODES.validationFailed;
    case 401:
      return API_ERROR_CODES.unauthenticated;
    case 403:
      return API_ERROR_CODES.forbidden;
    case 409:
      return API_ERROR_CODES.concurrencyConflict;
    case 429:
      return API_ERROR_CODES.rateLimited;
    default:
      return API_ERROR_CODES.unexpected;
  }
}

function fallbackMeta(traceId = ''): ApiMeta {
  return { traceId, timestamp: new Date().toISOString() };
}

function isEnvelope(value: unknown): value is ApiResponse<unknown> {
  if (typeof value !== 'object' || value === null) {
    return false;
  }

  return typeof (value as { success?: unknown }).success === 'boolean';
}

async function readEnvelope(response: Response): Promise<ApiResponse<unknown>> {
  try {
    const payload: unknown = await response.json();

    if (isEnvelope(payload)) {
      return payload;
    }
  } catch {
    // Non-JSON body (proxy error page, gateway timeout). Fall through to a
    // synthesized envelope so callers always receive a typed error.
  }

  return {
    success: false,
    data: null,
    error: {
      code: fallbackCodeForStatus(response.status),
      message: t('errors.unexpected'),
    },
    meta: fallbackMeta(response.headers.get('x-trace-id') ?? ''),
  };
}

function toRequestError(response: Response, envelope: ApiResponse<unknown>): ApiRequestError {
  if (!envelope.success && envelope.error) {
    const details: readonly ApiErrorDetail[] = envelope.error.details ?? [];

    return new ApiRequestError({
      code: envelope.error.code,
      message: envelope.error.message,
      status: response.status,
      traceId: envelope.meta.traceId,
      details,
    });
  }

  return new ApiRequestError({
    code: fallbackCodeForStatus(response.status),
    message: t('errors.unexpected'),
    status: response.status,
    traceId: envelope.meta.traceId,
  });
}

async function send(path: string, options: ApiRequestOptions): Promise<RawEnvelope> {
  const headers = new Headers(options.headers);
  headers.set('Accept', 'application/json');

  let body: BodyInit | undefined;

  if (options.body !== undefined) {
    headers.set('Content-Type', 'application/json');
    body = JSON.stringify(options.body);
  }

  let response: Response;

  try {
    response = await fetch(resolveRequestPath(path), {
      method: options.method ?? 'GET',
      headers,
      body,
      signal: options.signal,
      cache: options.cache ?? 'no-store',
      credentials: 'same-origin',
      redirect: 'error',
    });
  } catch (cause) {
    if (cause instanceof DOMException && cause.name === 'AbortError') {
      throw cause;
    }

    throw new ApiRequestError({
      code: API_ERROR_CODES.networkUnavailable,
      message: t('errors.network'),
      status: 0,
      traceId: '',
    });
  }

  return { envelope: await readEnvelope(response), response };
}

async function performRefresh(): Promise<boolean> {
  try {
    const { envelope, response } = await send(BFF_ROUTES.refresh, {
      method: 'POST',
      skipAuthRefresh: true,
    });

    return response.ok && envelope.success;
  } catch {
    return false;
  }
}

/**
 * Silently rotates the session cookies through the BFF. Concurrent callers
 * share one in-flight request so a burst of 401s produces a single refresh.
 */
export async function refreshSession(): Promise<boolean> {
  if (refreshPromise) {
    return refreshPromise;
  }

  refreshPromise = performRefresh();

  try {
    return await refreshPromise;
  } finally {
    refreshPromise = null;
  }
}

/**
 * Performs a request against the BFF, parses the standard envelope and
 * returns the success branch with `data`, `error: null` and `meta`.
 *
 * Failure behaviour:
 *  - `success: false` → throws `ApiRequestError` with code, message, traceId.
 *  - `401` → silent refresh once, retry once, then emit `unauthorized`.
 *  - `403 PLATFORM.MODULE_NOT_LICENSED` → emit `moduleNotLicensed` so the
 *    shell can refresh the session and render `ModuleNotLicensedState`.
 */
export async function apiRequest<T>(
  path: string,
  options: ApiRequestOptions = {},
): Promise<ApiSuccess<T>> {
  const { envelope, response } = await send(path, options);

  if (response.ok && envelope.success) {
    return envelope as ApiSuccess<T>;
  }

  const error = toRequestError(response, envelope);

  if (response.status === 401) {
    if (options.skipAuthRefresh) {
      throw error;
    }

    if (await refreshSession()) {
      return apiRequest<T>(path, { ...options, skipAuthRefresh: true });
    }

    emitApiEvent(API_EVENTS.unauthorized, error);
    throw error;
  }

  if (error.code === API_ERROR_CODES.moduleNotLicensed) {
    emitApiEvent(API_EVENTS.moduleNotLicensed, error);
    emitApiEvent(API_EVENTS.sessionRefresh, error);
  }

  throw error;
}

export const apiClient = {
  get<T>(path: string, options: Omit<ApiRequestOptions, 'method' | 'body'> = {}) {
    return apiRequest<T>(path, { ...options, method: 'GET' });
  },
  post<T>(path: string, body?: unknown, options: Omit<ApiRequestOptions, 'method' | 'body'> = {}) {
    return apiRequest<T>(path, { ...options, method: 'POST', body });
  },
  put<T>(path: string, body?: unknown, options: Omit<ApiRequestOptions, 'method' | 'body'> = {}) {
    return apiRequest<T>(path, { ...options, method: 'PUT', body });
  },
  patch<T>(path: string, body?: unknown, options: Omit<ApiRequestOptions, 'method' | 'body'> = {}) {
    return apiRequest<T>(path, { ...options, method: 'PATCH', body });
  },
  delete<T>(path: string, options: Omit<ApiRequestOptions, 'method' | 'body'> = {}) {
    return apiRequest<T>(path, { ...options, method: 'DELETE' });
  },
};
