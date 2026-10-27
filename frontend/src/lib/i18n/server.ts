import { cookies } from 'next/headers';

import {
  NEXT_LOCALE_COOKIE,
  resolveLocale,
  translate,
  type Locale,
  type TranslationKey,
  type TranslationValues,
} from './index';

/** Reads the persisted locale on the server so SSR copy matches the cookie. */
export async function getRequestLocale(): Promise<Locale> {
  const cookieStore = await cookies();
  return resolveLocale(cookieStore.get(NEXT_LOCALE_COOKIE)?.value);
}

export async function getServerTranslator(): Promise<{
  locale: Locale;
  t: (key: TranslationKey, values?: TranslationValues) => string;
}> {
  const locale = await getRequestLocale();

  return {
    locale,
    t: (key, values) => translate(locale, key, values),
  };
}
