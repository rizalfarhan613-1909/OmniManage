import type { ApiErrorDetail } from './types';

/**
 * Error codes used by the frontend. Backend codes mirror
 * `docs/error-codes.md`; `SYSTEM.NETWORK_UNAVAILABLE` is client-only and is
 * produced when the network request itself fails.
 */
export const API_ERROR_CODES = {
  unauthenticated: 'AUTH.UNAUTHENTICATED',
  tokenExpired: 'AUTH.TOKEN_EXPIRED',
  forbidden: 'AUTH.FORBIDDEN',
  moduleNotLicensed: 'PLATFORM.MODULE_NOT_LICENSED',
  validationFailed: 'VALIDATION.FAILED',
  concurrencyConflict: 'CONCURRENCY_CONFLICT',
  rateLimited: 'RATE_LIMIT.EXCEEDED',
  unexpected: 'SYSTEM.UNEXPECTED',
  networkUnavailable: 'SYSTEM.NETWORK_UNAVAILABLE',
} as const;

export interface ApiRequestErrorInit {
  code: string;
  message: string;
  status: number;
  traceId: string;
  details?: readonly ApiErrorDetail[];
}

/**
 * Typed failure thrown by the API client (Rules.md §6.8). The `traceId` is
 * always rendered by error UIs with a copy action so support can correlate.
 */
export class ApiRequestError extends Error {
  readonly code: string;
  readonly status: number;
  readonly traceId: string;
  readonly details: readonly ApiErrorDetail[];

  constructor({ code, message, status, traceId, details = [] }: ApiRequestErrorInit) {
    super(message);
    this.name = 'ApiRequestError';
    this.code = code;
    this.status = status;
    this.traceId = traceId;
    this.details = details;
  }

  isModuleNotLicensed(): boolean {
    return this.code === API_ERROR_CODES.moduleNotLicensed;
  }

  isUnauthenticated(): boolean {
    return (
      this.status === 401 ||
      this.code === API_ERROR_CODES.unauthenticated ||
      this.code === API_ERROR_CODES.tokenExpired
    );
  }

  isForbidden(): boolean {
    return this.status === 403;
  }
}
