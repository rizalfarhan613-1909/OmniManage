import { cookies } from 'next/headers';
import { cache } from 'react';

import { backendSession } from './backend';
import { ACCESS_TOKEN_COOKIE } from './cookies';
import type { Session } from './types';

/**
 * Reads the current session from the backend using the httpOnly access token.
 * Wrapped in React `cache` so a layout, guard and page in the same render pass
 * share a single network call.
 *
 * Server Components cannot rotate cookies, so an expired token returns `null`
 * and the shell redirects to `/login`. Browser-side refreshes go through
 * `GET /api/auth/session`, which can rotate the cookie pair.
 */
export const getSession = cache(async (): Promise<Session | null> => {
  const cookieStore = await cookies();
  const accessToken = cookieStore.get(ACCESS_TOKEN_COOKIE)?.value;

  if (!accessToken) {
    return null;
  }

  const result = await backendSession(accessToken);

  return result.envelope.success ? result.envelope.data : null;
});
