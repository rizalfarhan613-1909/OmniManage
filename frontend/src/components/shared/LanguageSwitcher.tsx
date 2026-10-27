'use client';

import { Check, ChevronDown, Globe2 } from 'lucide-react';
import { useEffect, useRef, useState } from 'react';

import { useI18n } from '@/components/providers/LanguageProvider';
import { LOCALE_META, SUPPORTED_LOCALES } from '@/lib/i18n';
import { cn } from '@/lib/utils/cn';

type Props = {
  align?: 'left' | 'right';
  className?: string;
};

/**
 * Compact EN/ID dropdown with flag + code indicators. Persists the choice in
 * `localStorage` and the `NEXT_LOCALE` cookie through `LanguageProvider`.
 */
export function LanguageSwitcher({ align = 'right', className }: Props) {
  const { locale, setLocale, t } = useI18n();
  const [isOpen, setIsOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);
  const buttonRef = useRef<HTMLButtonElement>(null);

  const active = LOCALE_META[locale];

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    const handlePointerDown = (event: MouseEvent) => {
      if (!containerRef.current?.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        setIsOpen(false);
        buttonRef.current?.focus();
      }
    };

    document.addEventListener('mousedown', handlePointerDown);
    document.addEventListener('keydown', handleKeyDown);

    return () => {
      document.removeEventListener('mousedown', handlePointerDown);
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [isOpen]);

  return (
    <div ref={containerRef} className={cn('relative', className)}>
      <button
        ref={buttonRef}
        type="button"
        onClick={() => setIsOpen((open) => !open)}
        aria-haspopup="menu"
        aria-expanded={isOpen}
        aria-label={t('common.languageLabel')}
        title={t('common.languageLabel')}
        className="inline-flex h-9 items-center gap-1.5 rounded-xl border border-line bg-surface px-2.5 text-[11px] font-semibold text-charcoal shadow-sm transition-colors duration-300 hover:border-cyan/40 hover:bg-surface-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary"
      >
        <Globe2 className="h-3.5 w-3.5 text-cyan-deep sm:hidden" aria-hidden="true" />
        <span className="text-sm leading-none" aria-hidden="true">
          {active.flag}
        </span>
        <span className="font-mono tracking-[0.08em]">{active.code}</span>
        <ChevronDown
          className={cn(
            'h-3 w-3 text-charcoal-light transition-transform duration-300',
            isOpen && 'rotate-180',
          )}
          aria-hidden="true"
        />
      </button>

      {isOpen ? (
        <div
          role="menu"
          aria-label={t('common.languageLabel')}
          className={cn(
            'absolute top-full z-40 mt-2 w-44 overflow-hidden rounded-xl border border-line bg-surface p-1 shadow-md',
            align === 'right' ? 'right-0' : 'left-0',
          )}
        >
          {SUPPORTED_LOCALES.map((option) => {
            const meta = LOCALE_META[option];
            const isActive = option === locale;

            return (
              <button
                key={option}
                type="button"
                role="menuitemradio"
                aria-checked={isActive}
                onClick={() => {
                  setLocale(option);
                  setIsOpen(false);
                }}
                className={cn(
                  'flex w-full items-center gap-2.5 rounded-lg px-2.5 py-2 text-left text-xs font-medium transition-colors duration-200',
                  isActive
                    ? 'bg-cyan/10 text-charcoal-strong'
                    : 'text-charcoal-light hover:bg-surface-muted hover:text-charcoal',
                )}
              >
                <span className="text-base leading-none" aria-hidden="true">
                  {meta.flag}
                </span>
                <span className="min-w-0 flex-1 truncate">{t(meta.labelKey)}</span>
                <span className="font-mono text-[10px] font-bold text-charcoal-light">
                  {meta.code}
                </span>
                {isActive ? (
                  <Check className="h-3.5 w-3.5 text-cyan-deep" aria-hidden="true" />
                ) : null}
              </button>
            );
          })}
        </div>
      ) : null}
    </div>
  );
}
