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
  LOCALE_COOKIE_MAX_AGE,
  LOCALE_STORAGE_KEY,
  NEXT_LOCALE_COOKIE,
  isLocale,
  translate,
  type Locale,
  type TranslationKey,
  type TranslationValues,
} from '@/lib/i18n';

type Translator = (key: TranslationKey, values?: TranslationValues) => string;

type LanguageContextValue = {
  locale: Locale;
  /** BCP-47 locale for Intl formatters (`en-US` / `id-ID`). */
  intlLocale: string;
  t: Translator;
  setLocale: (locale: Locale) => void;
  toggleLocale: () => void;
};

const INTL_LOCALE: Record<Locale, string> = { en: 'en-US', id: 'id-ID' };

const LanguageContext = createContext<LanguageContextValue>({
  locale: 'en',
  intlLocale: INTL_LOCALE.en,
  t: (key, values) => translate('en', key, values),
  setLocale: () => undefined,
  toggleLocale: () => undefined,
});

function readStoredLocale(): Locale | null {
  try {
    const stored = window.localStorage.getItem(LOCALE_STORAGE_KEY);
    return isLocale(stored) ? stored : null;
  } catch {
    return null;
  }
}

const getServerStoredLocale = (): Locale | null => null;

function subscribeToStorage(onChange: () => void): () => void {
  window.addEventListener('storage', onChange);
  return () => window.removeEventListener('storage', onChange);
}

function writeLocaleCookie(locale: Locale): void {
  document.cookie = `${NEXT_LOCALE_COOKIE}=${locale}; path=/; max-age=${LOCALE_COOKIE_MAX_AGE}; SameSite=Lax`;
}

function persistLocale(locale: Locale): void {
  try {
    window.localStorage.setItem(LOCALE_STORAGE_KEY, locale);
  } catch {
    // Storage can be unavailable in private mode; the cookie still works.
  }

  document.documentElement.lang = locale;
  writeLocaleCookie(locale);
}

/**
 * Lightweight EN/ID provider.
 *
 * - Server passes the `NEXT_LOCALE` cookie value as `initialLocale` so SSR and
 *   hydration match.
 * - A locale previously written to `localStorage` takes over after hydration
 *   (the cookie is the transport for the BFF `Accept-Language` header).
 * - Changing locale updates `<html lang>`, `localStorage` and the cookie.
 */
export function LanguageProvider({
  initialLocale,
  children,
}: {
  initialLocale: Locale;
  children: ReactNode;
}) {
  // Explicit choice wins; otherwise fall back to storage, then the server cookie.
  const [selectedLocale, setSelectedLocale] = useState<Locale | null>(null);
  const storedLocale = useSyncExternalStore(
    subscribeToStorage,
    readStoredLocale,
    getServerStoredLocale,
  );
  const locale = selectedLocale ?? storedLocale ?? initialLocale;

  useEffect(() => {
    document.documentElement.lang = locale;

    // A locale adopted from `localStorage` (cookie expired or cleared) must also
    // be written back so the BFF keeps forwarding the matching `Accept-Language`.
    if (locale !== initialLocale) {
      writeLocaleCookie(locale);
    }
  }, [initialLocale, locale]);

  const setLocale = useCallback((next: Locale) => {
    setSelectedLocale(next);
    persistLocale(next);
  }, []);

  const toggleLocale = useCallback(() => {
    setLocale(locale === 'en' ? 'id' : 'en');
  }, [locale, setLocale]);

  const t = useCallback<Translator>((key, values) => translate(locale, key, values), [locale]);

  const value = useMemo<LanguageContextValue>(
    () => ({
      locale,
      intlLocale: INTL_LOCALE[locale],
      t,
      setLocale,
      toggleLocale,
    }),
    [locale, setLocale, t, toggleLocale],
  );

  return <LanguageContext.Provider value={value}>{children}</LanguageContext.Provider>;
}

export function useI18n(): LanguageContextValue {
  return useContext(LanguageContext);
}
