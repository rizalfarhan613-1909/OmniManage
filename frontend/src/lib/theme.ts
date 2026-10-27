/** Browser theme preference and the pre-hydration FOUC guard script. */
export const THEME_STORAGE_KEY = 'omnimanage-theme';
export const THEME_ATTRIBUTE = 'data-theme';

export type ThemePreference = 'light' | 'dark' | 'system';
export type ResolvedTheme = 'light' | 'dark';

export function isThemePreference(value: unknown): value is ThemePreference {
  return value === 'light' || value === 'dark' || value === 'system';
}

/**
 * Runs before paint (injected by `next/script` with `beforeInteractive`) so the
 * first frame already matches the persisted theme or the OS preference.
 */
export const THEME_INIT_SCRIPT = `(function(){try{var k=${JSON.stringify(
  THEME_STORAGE_KEY,
)};var stored=localStorage.getItem(k);var prefersDark=window.matchMedia('(prefers-color-scheme: dark)').matches;var dark=stored==='dark'||((!stored||stored==='system')&&prefersDark);var root=document.documentElement;root.classList.toggle('dark',dark);root.style.colorScheme=dark?'dark':'light';root.setAttribute(${JSON.stringify(
  THEME_ATTRIBUTE,
)},dark?'dark':'light');}catch(e){}})();`;
