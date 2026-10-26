'use client';

import { Loader2 } from 'lucide-react';

import { t } from '@/lib/i18n';
import { cn } from '@/lib/utils/cn';

type Props = {
  checked: boolean;
  label: string;
  descriptionId?: string;
  isPending?: boolean;
  disabled?: boolean;
  onToggle: () => void;
};

/**
 * Tactile module license switch (DESIGN.md §2.4 / §3.4).
 *
 * Mint `#8AFFC9` when active, Sage `#C9DAD3` when inactive, plus an explicit
 * ON/OFF label and `role="switch"` so status never depends on colour alone.
 */
export function ModuleLicenseSwitch({
  checked,
  label,
  descriptionId,
  isPending = false,
  disabled = false,
  onToggle,
}: Props) {
  const isDisabled = disabled || isPending;

  return (
    <button
      type="button"
      role="switch"
      aria-checked={checked}
      aria-label={label}
      aria-describedby={descriptionId}
      aria-busy={isPending}
      disabled={isDisabled}
      onClick={onToggle}
      className={cn(
        'group inline-flex items-center gap-2 rounded-md px-1 py-1 transition-opacity duration-300 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary focus-visible:ring-offset-2',
        isDisabled && 'cursor-not-allowed opacity-60',
      )}
    >
      <span
        aria-hidden="true"
        className={cn(
          'relative inline-flex h-6 w-11 shrink-0 items-center rounded-full border transition-colors duration-300',
          checked ? 'border-mint-accent bg-mint-accent' : 'border-sage bg-sage',
        )}
      >
        <span
          className={cn(
            'inline-block h-5 w-5 transform rounded-full bg-surface shadow-sm transition-transform duration-200 ease-spring',
            checked ? 'translate-x-[1.375rem]' : 'translate-x-0.5',
          )}
        />
      </span>
      <span className="inline-flex min-w-[3.25rem] items-center gap-1 text-xs font-semibold uppercase tracking-wide text-charcoal">
        {isPending ? <Loader2 className="h-3.5 w-3.5 animate-spin" aria-hidden="true" /> : null}
        {isPending
          ? t('admin.licensing.matrix.applying')
          : checked
            ? t('admin.licensing.matrix.on')
            : t('admin.licensing.matrix.off')}
      </span>
    </button>
  );
}
