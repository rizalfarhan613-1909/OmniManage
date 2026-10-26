'use client';

import { useRouter } from 'next/navigation';
import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from 'react';

import { apiClient } from '@/lib/api/client';
import { ApiRequestError } from '@/lib/api/errors';
import { API_EVENTS } from '@/lib/api/events';
import { BFF_ROUTES } from '@/lib/api/routes';
import type { Session } from '@/lib/auth/types';

/** DESIGN.md §3.2 rule 2: re-validate on focus and every 5 minutes. */
const SESSION_REFRESH_INTERVAL_MS = 5 * 60 * 1000;
const LOGIN_ROUTE = '/login';

const SessionContext = createContext<Session | null>(null);
const SessionRefreshContext = createContext<(() => Promise<void>) | null>(null);

type Props = {
  initialSession: Session;
  children: ReactNode;
};

/**
 * Holds the server-validated session (active modules + permissions) in memory.
 * The session payload contains no tokens — those stay in httpOnly cookies.
 */
export function SessionProvider({ initialSession, children }: Props) {
  const [session, setSession] = useState<Session>(initialSession);
  const router = useRouter();

  const refresh = useCallback(async () => {
    try {
      const result = await apiClient.get<Session>(BFF_ROUTES.session, {
        skipAuthRefresh: true,
      });
      setSession(result.data);
    } catch (error) {
      if (error instanceof ApiRequestError && error.isUnauthenticated()) {
        router.replace(LOGIN_ROUTE);
        return;
      }

      // Transient failures keep the last known session; the next interval or
      // focus event retries without interrupting the user.
      console.warn('Session refresh failed.', error);
    }
  }, [router]);

  useEffect(() => {
    const intervalId = window.setInterval(() => {
      void refresh();
    }, SESSION_REFRESH_INTERVAL_MS);

    const handleFocus = () => {
      void refresh();
    };

    const handleVisibilityChange = () => {
      if (document.visibilityState === 'visible') {
        void refresh();
      }
    };

    const handleUnauthorized = () => {
      router.replace(LOGIN_ROUTE);
    };

    const handleRefreshRequest = () => {
      void refresh();
    };

    window.addEventListener('focus', handleFocus);
    document.addEventListener('visibilitychange', handleVisibilityChange);
    window.addEventListener(API_EVENTS.unauthorized, handleUnauthorized);
    window.addEventListener(API_EVENTS.sessionRefresh, handleRefreshRequest);

    return () => {
      window.clearInterval(intervalId);
      window.removeEventListener('focus', handleFocus);
      document.removeEventListener('visibilitychange', handleVisibilityChange);
      window.removeEventListener(API_EVENTS.unauthorized, handleUnauthorized);
      window.removeEventListener(API_EVENTS.sessionRefresh, handleRefreshRequest);
    };
  }, [refresh, router]);

  return (
    <SessionContext.Provider value={session}>
      <SessionRefreshContext.Provider value={refresh}>{children}</SessionRefreshContext.Provider>
    </SessionContext.Provider>
  );
}

export function useSession(): Session {
  const session = useContext(SessionContext);

  if (!session) {
    throw new Error('useSession must be used inside SessionProvider.');
  }

  return session;
}

/**
 * Imperative session revalidation for actions that change the tenant's
 * licenses or permissions (DESIGN.md §3.2 rule 2). Re-fetches
 * `GET /api/auth/session` and re-renders the sidebar immediately.
 */
export function useSessionRefresh(): () => Promise<void> {
  const refresh = useContext(SessionRefreshContext);

  if (!refresh) {
    throw new Error('useSessionRefresh must be used inside SessionProvider.');
  }

  return refresh;
}
