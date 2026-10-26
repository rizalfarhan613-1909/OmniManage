/**
 * Server-side environment access. Only imported by route handlers, server
 * components and server-only helpers — never by client components.
 */
const DEFAULT_API_BASE_URL = 'http://localhost:5000';
const REQUEST_TIMEOUT_MS = 15_000;

export function getApiBaseUrl(): string {
  const configured = process.env.API_BASE_URL?.trim();
  const baseUrl = configured && configured.length > 0 ? configured : DEFAULT_API_BASE_URL;

  try {
    new URL(baseUrl);
  } catch {
    throw new Error('API_BASE_URL must be an absolute URL.');
  }

  return baseUrl.replace(/\/+$/, '');
}

export function getBackendTimeoutMs(): number {
  return REQUEST_TIMEOUT_MS;
}
