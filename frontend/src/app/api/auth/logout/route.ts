import { NextResponse, type NextRequest } from 'next/server';

import { backendLogout } from '@/lib/auth/backend';
import { REFRESH_TOKEN_COOKIE, clearSessionCookies } from '@/lib/auth/cookies';
import { assertSameOrigin, bffMeta } from '@/lib/api/bff';
import type { ApiResponse } from '@/lib/api/types';

interface LogoutResult {
  loggedOut: true;
}

/**
 * Revokes the backend session family (best effort) and always clears the
 * BFF cookies so the browser cannot continue with a stale session.
 */
export async function POST(request: NextRequest): Promise<NextResponse> {
  const csrfError = assertSameOrigin(request);

  if (csrfError) {
    return csrfError;
  }

  const refreshToken = request.cookies.get(REFRESH_TOKEN_COOKIE)?.value;

  if (refreshToken) {
    await backendLogout(refreshToken);
  }

  const meta = bffMeta();
  const response = NextResponse.json<ApiResponse<LogoutResult>>(
    { success: true, data: { loggedOut: true }, error: null, meta },
    { status: 200, headers: { 'x-trace-id': meta.traceId } },
  );

  clearSessionCookies(response);

  return response;
}
