'use client';

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  useSyncExternalStore,
  type ReactNode,
} from 'react';

import {
  THEME_ATTRIBUTE,
  THEME_STORAGE_KEY,
  isThemePreference,
  type ResolvedTheme,
  type ThemePreference,
} from '@/lib/theme';

type ThemeContextValue = {
  /** Stored preference (may be `system`). */
  theme: ThemePreference;
  /** Concrete theme currently applied to the document. */
  resolvedTheme: ResolvedTheme;
  /** True after the client has hydrated; use to avoid icon flicker. */
  mounted: boolean;
  setTheme: (theme: ThemePreference) => void;
  toggleTheme: () => void;
};

const ThemeContext = createContext<ThemeContextValue>({
  theme: 'system',
  resolvedTheme: 'light',
  mounted: false,
  setTheme: () => undefined,
  toggleTheme: () => undefined,
});

/** Hydration signal: `false` for SSR/hydration, `true` once the client owns the tree. */
const subscribeToHydration = () => () => undefined;
const getHydratedSnapshot = () => true;
const getServerHydratedSnapshot = () => false;

function readStoredTheme(): ThemePreference | null {
  try {
    const stored = window.localStorage.getItem(THEME_STORAGE_KEY);
    return isThemePreference(stored) ? stored : null;
  } catch {
    return null;
  }
}

function systemPrefersDark(): boolean {
  return typeof window !== 'undefined' && window.matchMedia('(prefers-color-scheme: dark)').matches;
}

function readInitialTheme(): ThemePreference {
  if (typeof window === 'undefined') {
    return 'system';
  }

  return readStoredTheme() ?? 'system';
}

function resolveTheme(theme: ThemePreference): ResolvedTheme {
  return theme === 'dark' || (theme === 'system' && systemPrefersDark()) ? 'dark' : 'light';
}

function readInitialResolvedTheme(): ResolvedTheme {
  if (typeof window === 'undefined') {
    return 'light';
  }

  return resolveTheme(readInitialTheme());
}

function applyTheme(theme: ThemePreference): ResolvedTheme {
  const resolved = resolveTheme(theme);
  const root = document.documentElement;
  root.classList.toggle('dark', resolved === 'dark');
  root.style.colorScheme = resolved;
  root.setAttribute(THEME_ATTRIBUTE, resolved);

  return resolved;
}

/**
 * Theme state with `localStorage` persistence and system-preference fallback.
 *
 * This is the `next-themes` contract implemented locally: no flash of the
 * wrong theme (see `THEME_INIT_SCRIPT`), cross-tab sync, and live OS changes
 * while the preference is `system`.
 */
export function ThemeProvider({ children }: { children: ReactNode }) {
  const [theme, setThemeState] = useState<ThemePreference>(readInitialTheme);
  const [resolvedTheme, setResolvedTheme] = useState<ResolvedTheme>(readInitialResolvedTheme);
  const mounted = useSyncExternalStore(
    subscribeToHydration,
    getHydratedSnapshot,
    getServerHydratedSnapshot,
  );

  const setTheme = useCallback((next: ThemePreference) => {
    try {
      window.localStorage.setItem(THEME_STORAGE_KEY, next);
    } catch {
      // Private-mode storage failures must not break theme switching.
    }

    setThemeState(next);
    setResolvedTheme(applyTheme(next));
  }, []);

  const toggleTheme = useCallback(() => {
    setTheme(resolvedTheme === 'dark' ? 'light' : 'dark');
  }, [resolvedTheme, setTheme]);

  // Keep the document in sync with the selected preference (external system only,
  // so this never causes a cascading render).
  useEffect(() => {
    applyTheme(theme);
  }, [theme]);

  // Follow live OS changes while the preference is `system`.
  useEffect(() => {
    if (theme !== 'system') {
      return;
    }

    const query = window.matchMedia('(prefers-color-scheme: dark)');
    const handleChange = () => setResolvedTheme(applyTheme('system'));

    query.addEventListener?.('change', handleChange);
    return () => query.removeEventListener?.('change', handleChange);
  }, [theme]);

  // Mirror changes made in other tabs.
  useEffect(() => {
    const handleStorage = (event: StorageEvent) => {
      if (event.key !== THEME_STORAGE_KEY) {
        return;
      }

      const next = isThemePreference(event.newValue) ? event.newValue : 'system';
      setThemeState(next);
      setResolvedTheme(applyTheme(next));
    };

    window.addEventListener('storage', handleStorage);
    return () => window.removeEventListener('storage', handleStorage);
  }, []);

  const value = useMemo<ThemeContextValue>(
    () => ({ theme, resolvedTheme, mounted, setTheme, toggleTheme }),
    [mounted, resolvedTheme, setTheme, theme, toggleTheme],
  );

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
}

export function useTheme(): ThemeContextValue {
  return useContext(ThemeContext);
}
