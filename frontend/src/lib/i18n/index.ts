import { en, type Dictionary, type TranslationKey, type TranslationValues } from './en';
import { id } from './id';

export type { Dictionary, TranslationKey, TranslationValues };

export type Locale = 'en' | 'id';

/** English stays the source of truth for the key union. */
export const DEFAULT_LOCALE: Locale = 'en';

export const SUPPORTED_LOCALES: readonly Locale[] = ['en', 'id'];

/** Browser storage + BFF cookie names (cookie is forwarded as Accept-Language). */
export const LOCALE_STORAGE_KEY = 'omnimanage-locale';
export const NEXT_LOCALE_COOKIE = 'NEXT_LOCALE';
export const LOCALE_COOKIE_MAX_AGE = 60 * 60 * 24 * 365;

export const dictionaries: Record<Locale, Dictionary> = { en, id };

export const LOCALE_META: Record<Locale, { code: string; flag: string; labelKey: TranslationKey }> =
  {
    en: { code: 'EN', flag: '🇺🇸', labelKey: 'common.english' },
    id: { code: 'ID', flag: '🇮🇩', labelKey: 'common.indonesian' },
  };

export function isLocale(value: unknown): value is Locale {
  return value === 'en' || value === 'id';
}

/** Resolves an arbitrary cookie/header value to a supported locale. */
export function resolveLocale(value: string | null | undefined): Locale {
  if (isLocale(value)) {
    return value;
  }

  const normalized = value?.trim().toLowerCase() ?? '';

  if (normalized.startsWith('id') || normalized.startsWith('in')) {
    return 'id';
  }

  return DEFAULT_LOCALE;
}

/** Value used in the `Accept-Language` header forwarded to backend APIs. */
export function acceptLanguageFor(locale: Locale): string {
  return locale === 'id' ? 'id-ID,id;q=0.9,en;q=0.8' : 'en-US,en;q=0.9,id;q=0.8';
}

export function interpolate(template: string, values?: TranslationValues): string {
  if (!values) {
    return template;
  }

  return template.replace(/\{(\w+)\}/g, (match, name: string) => {
    const value = values[name];
    return value === undefined ? match : String(value);
  });
}

/**
 * Type-safe translator for a given locale. Falls back to English if a
 * dictionary ever drifts, which keeps the UI readable instead of blank.
 */
export function translate(locale: Locale, key: TranslationKey, values?: TranslationValues): string {
  const template = dictionaries[locale]?.[key] ?? en[key];
  return interpolate(template, values);
}

/**
 * Server-side default translator. Client components use `useI18n()` from
 * `LanguageProvider` so their copy updates live when the locale changes.
 */
export function t(key: TranslationKey, values?: TranslationValues): string {
  return translate(DEFAULT_LOCALE, key, values);
}

export { en, id };
