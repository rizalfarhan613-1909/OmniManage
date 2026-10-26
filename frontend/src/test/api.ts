import { vi } from 'vitest';

export type FetchLike = (input: string | URL | Request, init?: RequestInit) => Promise<Response>;

const META = { traceId: 'trace-test-001', timestamp: '2026-10-22T08:00:00Z' };

/** Builds a standard success envelope matching the BFF contract. */
export function successResponse<T>(data: T, status = 200): Response {
  return new Response(JSON.stringify({ success: true, data, error: null, meta: META }), {
    status,
    headers: { 'Content-Type': 'application/json' },
  });
}

/** Builds a standard failure envelope matching the BFF contract. */
export function errorResponse(
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

/** Creates a typed `fetch` mock for hook-level integration tests. */
export function createFetchMock(): ReturnType<typeof vi.fn<FetchLike>> {
  return vi.fn<FetchLike>();
}
