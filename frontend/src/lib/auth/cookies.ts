import type { NextResponse } from 'next/server';

/**
 * BFF session cookies (Security.md §2.5).
 * Tokens never reach browser JavaScript: httpOnly + Secure + SameSite=Lax.
 */
export const ACCESS_TOKEN_COOKIE = 'omni_access_token';
export const REFRESH_TOKEN_COOKIE = 'omni_refresh_token';

const COOKIE_PATH = '/';

/**
 * Security.md §2.3: web refresh sessions are limited to 12 hours of idle time
 * even if the backend issues a longer absolute refresh lifetime. The browser
 * cookie is the effective web session, and it is renewed on every rotation.
 */
const WEB_REFRESH_IDLE_SECONDS = 12 * 60 * 60;

export interface SessionTokens {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
}

function maxAgeSeconds(expiresAt: string): number {
  const expiresAtMs = Date.parse(expiresAt);
  const remaining = Number.isNaN(expiresAtMs) ? 0 : Math.floor((expiresAtMs - Date.now()) / 1000);

  return Math.max(remaining, 1);
}

function baseOptions(maxAge: number) {
  return {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax' as const,
    path: COOKIE_PATH,
    maxAge,
  };
}

export function setSessionCookies(response: NextResponse, tokens: SessionTokens): void {
  response.cookies.set(
    ACCESS_TOKEN_COOKIE,
    tokens.accessToken,
    baseOptions(maxAgeSeconds(tokens.accessTokenExpiresAt)),
  );
  response.cookies.set(
    REFRESH_TOKEN_COOKIE,
    tokens.refreshToken,
    baseOptions(Math.min(maxAgeSeconds(tokens.refreshTokenExpiresAt), WEB_REFRESH_IDLE_SECONDS)),
  );
}

export function clearSessionCookies(response: NextResponse): void {
  response.cookies.set(ACCESS_TOKEN_COOKIE, '', baseOptions(0));
  response.cookies.set(REFRESH_TOKEN_COOKIE, '', baseOptions(0));
}
