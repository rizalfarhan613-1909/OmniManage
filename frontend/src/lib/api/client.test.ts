import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { apiClient, refreshSession } from '@/lib/api/client';
import { API_ERROR_CODES, ApiRequestError } from '@/lib/api/errors';
import { API_EVENTS } from '@/lib/api/events';
import { BFF_ROUTES } from '@/lib/api/routes';

type FetchLike = (input: string | URL | Request, init?: RequestInit) => Promise<Response>;

const META = { traceId: 'trace-001', timestamp: '2026-10-22T08:00:00Z' };

function successResponse<T>(data: T, status = 200): Response {
  return new Response(JSON.stringify({ success: true, data, error: null, meta: META }), {
    status,
    headers: { 'Content-Type': 'application/json' },
  });
}

function errorResponse(
  code: string,
  message: string,
  status: number,
  traceId = META.traceId,
): Response {
  return new Response(
    JSON.stringify({
      success: false,
      data: null,
      error: { code, message },
      meta: { traceId, timestamp: META.timestamp },
    }),
    { status, headers: { 'Content-Type': 'application/json' } },
  );
}

function htmlResponse(status: number): Response {
  return new Response('<html>Bad gateway</html>', {
    status,
    headers: { 'Content-Type': 'text/html' },
  });
}

let fetchMock: ReturnType<typeof vi.fn<FetchLike>>;

beforeEach(() => {
  fetchMock = vi.fn<FetchLike>();
  vi.stubGlobal('fetch', fetchMock);
});

afterEach(() => {
  vi.unstubAllGlobals();
});

describe('apiClient envelope handling', () => {
  it('returns data and meta for a success envelope', async () => {
    fetchMock.mockResolvedValue(successResponse({ id: 'so-1' }));

    const result = await apiClient.get<{ id: string }>('/api/v1/sales/orders/so-1');

    expect(result.data).toEqual({ id: 'so-1' });
    expect(result.error).toBeNull();
    expect(result.meta.traceId).toBe(META.traceId);
  });

  it('routes backend paths through the BFF proxy', async () => {
    fetchMock.mockResolvedValue(successResponse([]));

    await apiClient.get('/api/v1/sales/orders?page=2');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/proxy/sales/orders?page=2',
      expect.objectContaining({ method: 'GET', credentials: 'same-origin' }),
    );
  });

  it('throws a typed ApiRequestError when success is false', async () => {
    fetchMock.mockResolvedValue(
      errorResponse('SALES.ORDER.INSUFFICIENT_CREDIT', 'Credit limit exceeded.', 409, 'trace-9'),
    );

    const failure = apiClient.get('/api/v1/sales/orders/so-1');

    await expect(failure).rejects.toBeInstanceOf(ApiRequestError);
    await expect(failure).rejects.toMatchObject({
      code: 'SALES.ORDER.INSUFFICIENT_CREDIT',
      message: 'Credit limit exceeded.',
      status: 409,
      traceId: 'trace-9',
    });
  });

  it('maps a non-JSON gateway failure to a typed unexpected error', async () => {
    fetchMock.mockResolvedValue(htmlResponse(502));

    await expect(apiClient.get('/api/v1/sales/orders')).rejects.toMatchObject({
      code: API_ERROR_CODES.unexpected,
      status: 502,
    });
  });
});

describe('apiClient auth handling', () => {
  it('refreshes once and retries after a 401', async () => {
    fetchMock
      .mockResolvedValueOnce(errorResponse(API_ERROR_CODES.tokenExpired, 'Expired', 401))
      .mockResolvedValueOnce(successResponse({}))
      .mockResolvedValueOnce(successResponse({ id: 'so-1' }));

    const result = await apiClient.get<{ id: string }>('/api/v1/sales/orders/so-1');

    expect(result.data).toEqual({ id: 'so-1' });
    expect(fetchMock).toHaveBeenCalledTimes(3);
    expect(fetchMock.mock.calls[1]?.[0]).toBe(BFF_ROUTES.refresh);
  });

  it('shares one refresh between concurrent callers', async () => {
    fetchMock
      .mockResolvedValueOnce(errorResponse(API_ERROR_CODES.tokenExpired, 'Expired', 401))
      .mockResolvedValueOnce(errorResponse(API_ERROR_CODES.tokenExpired, 'Expired', 401))
      .mockResolvedValueOnce(successResponse({}))
      .mockResolvedValueOnce(successResponse({ id: 1 }))
      .mockResolvedValueOnce(successResponse({ id: 2 }));

    const [first, second] = await Promise.all([
      apiClient.get<{ id: number }>('/api/v1/sales/orders/1'),
      apiClient.get<{ id: number }>('/api/v1/sales/orders/2'),
    ]);

    expect(first.data.id).toBe(1);
    expect(second.data.id).toBe(2);
    expect(fetchMock.mock.calls.filter(([url]) => url === BFF_ROUTES.refresh)).toHaveLength(1);
  });

  it('emits unauthorized when the refresh also fails', async () => {
    const listener = vi.fn();
    window.addEventListener(API_EVENTS.unauthorized, listener);
    fetchMock
      .mockResolvedValueOnce(errorResponse(API_ERROR_CODES.tokenExpired, 'Expired', 401))
      .mockResolvedValueOnce(errorResponse(API_ERROR_CODES.unauthenticated, 'No session', 401));

    await expect(apiClient.get('/api/v1/sales/orders')).rejects.toBeInstanceOf(ApiRequestError);

    expect(listener).toHaveBeenCalledTimes(1);
    window.removeEventListener(API_EVENTS.unauthorized, listener);
  });

  it('does not attempt a refresh when skipAuthRefresh is set (login)', async () => {
    const unauthorizedListener = vi.fn();
    window.addEventListener(API_EVENTS.unauthorized, unauthorizedListener);
    fetchMock.mockResolvedValueOnce(
      errorResponse(API_ERROR_CODES.unauthenticated, 'Invalid credentials', 401),
    );

    await expect(
      apiClient.post(
        BFF_ROUTES.login,
        { tenantCode: 'ACME', email: 'a@b.co', password: 'x' },
        { skipAuthRefresh: true },
      ),
    ).rejects.toMatchObject({ code: API_ERROR_CODES.unauthenticated });

    expect(fetchMock).toHaveBeenCalledTimes(1);
    expect(unauthorizedListener).not.toHaveBeenCalled();
    window.removeEventListener(API_EVENTS.unauthorized, unauthorizedListener);
  });

  it('emits module-not-licensed and requests a session refresh on 403', async () => {
    const licensedListener = vi.fn();
    const sessionListener = vi.fn();
    window.addEventListener(API_EVENTS.moduleNotLicensed, licensedListener);
    window.addEventListener(API_EVENTS.sessionRefresh, sessionListener);

    fetchMock.mockResolvedValueOnce(
      errorResponse(API_ERROR_CODES.moduleNotLicensed, 'Module not licensed.', 403),
    );

    await expect(apiClient.get('/api/v1/finance/invoices')).rejects.toMatchObject({
      code: API_ERROR_CODES.moduleNotLicensed,
      status: 403,
    });

    expect(licensedListener).toHaveBeenCalledTimes(1);
    expect(sessionListener).toHaveBeenCalledTimes(1);

    window.removeEventListener(API_EVENTS.moduleNotLicensed, licensedListener);
    window.removeEventListener(API_EVENTS.sessionRefresh, sessionListener);
  });
});

describe('refreshSession', () => {
  it('returns false when the refresh endpoint fails', async () => {
    fetchMock.mockResolvedValueOnce(
      errorResponse(API_ERROR_CODES.unauthenticated, 'No session', 401),
    );

    await expect(refreshSession()).resolves.toBe(false);
  });

  it('returns true when cookies are rotated', async () => {
    fetchMock.mockResolvedValueOnce(successResponse({}));

    await expect(refreshSession()).resolves.toBe(true);
  });
});
