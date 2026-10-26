/**
 * Same-origin BFF endpoints. The browser only ever talks to these routes;
 * backend JWTs stay in httpOnly cookies (Security.md §2.5).
 */
export const BFF_ROUTES = {
  login: '/api/auth/login',
  logout: '/api/auth/logout',
  refresh: '/api/auth/refresh',
  session: '/api/auth/session',
} as const;

const BACKEND_API_PREFIX = '/api/v1/';
const BFF_PROXY_PREFIX = '/api/proxy/';

/**
 * Rewrites documented backend paths (`/api/v1/sales/orders`) onto the BFF
 * proxy so feature code never has to know about the proxy layout.
 */
export function resolveRequestPath(path: string): string {
  return path.startsWith(BACKEND_API_PREFIX)
    ? `${BFF_PROXY_PREFIX}${path.slice(BACKEND_API_PREFIX.length)}`
    : path;
}
