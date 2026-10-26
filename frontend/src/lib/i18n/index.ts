import { en, type TranslationKey, type TranslationValues } from './en';

export type { TranslationKey, TranslationValues };

/**
 * Minimal, type-safe translator. The key set is derived from the English
 * dictionary, so a missing or misspelled key is a compile-time error.
 */
export function t(key: TranslationKey, values?: TranslationValues): string {
  const template: string = en[key];

  if (!values) {
    return template;
  }

  return template.replace(/\{(\w+)\}/g, (match, name: string) => {
    const value = values[name];
    return value === undefined ? match : String(value);
  });
}

export { en };
