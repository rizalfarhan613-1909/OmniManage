'use client';

import { Moon, Sun } from 'lucide-react';

import { useTheme } from '@/components/providers/ThemeProvider';
import { useI18n } from '@/components/providers/LanguageProvider';
import { cn } from '@/lib/utils/cn';

type Props = {
  className?: string;
};

/**
 * Compact light/dark toggle. The icon and every theme-dependent attribute are
 * only rendered after hydration so the server-rendered markup never disagrees
 * with the persisted preference read from `localStorage`/the OS.
 */
export function ThemeToggle({ className }: Props) {
  const { t } = useI18n();
  const { resolvedTheme, toggleTheme, mounted } = useTheme();
  const isDark = resolvedTheme === 'dark';

  // Before hydration the active theme is unknown, so advertise the neutral
  // action rather than guessing. Server HTML and the first client render agree.
  const label = !mounted
    ? t('common.themeToggle')
    : isDark
      ? t('common.themeLightHint')
      : t('common.themeDarkHint');

  return (
    <button
      type="button"
      onClick={toggleTheme}
      aria-label={label}
      title={label}
      aria-pressed={mounted ? isDark : undefined}
      className={cn(
        'inline-flex h-9 w-9 shrink-0 items-center justify-center rounded-xl border border-line bg-surface text-charcoal-light shadow-sm transition-colors duration-300 hover:border-cyan/40 hover:bg-surface-muted hover:text-charcoal-strong focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary',
        className,
      )}
    >
      {mounted ? (
        isDark ? (
          <Sun className="h-4 w-4" aria-hidden="true" />
        ) : (
          <Moon className="h-4 w-4" aria-hidden="true" />
        )
      ) : (
        <span className="h-4 w-4" aria-hidden="true" />
      )}
    </button>
  );
}
