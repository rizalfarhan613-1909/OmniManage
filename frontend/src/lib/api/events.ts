/**
 * Browser events emitted by the API layer. Keeping them in one place avoids
 * string typos between the client, the session provider and error boundaries.
 */
export const API_EVENTS = {
  /** Session refresh failed — the user must sign in again. */
  unauthorized: 'omni:auth-unauthorized',
  /** The backend returned 403 PLATFORM.MODULE_NOT_LICENSED. */
  moduleNotLicensed: 'omni:module-not-licensed',
  /** Active modules/permissions may have changed; re-fetch the session. */
  sessionRefresh: 'omni:session-refresh',
} as const;

export type ApiEventName = (typeof API_EVENTS)[keyof typeof API_EVENTS];

export function emitApiEvent(name: ApiEventName, detail?: unknown): void {
  if (typeof window === 'undefined') {
    return;
  }

  window.dispatchEvent(new CustomEvent(name, { detail }));
}
