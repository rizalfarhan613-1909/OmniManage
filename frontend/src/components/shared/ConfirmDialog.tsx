'use client';

import { useEffect, useId, useRef, type ReactNode } from 'react';

import { useI18n } from '@/components/providers/LanguageProvider';
import { ErrorState } from '@/components/shared/ErrorState';
import { Button } from '@/components/ui/Button';
import { cn } from '@/lib/utils/cn';

type Props = {
  isOpen: boolean;
  title: string;
  description: string;
  confirmLabel: string;
  pendingLabel: string;
  cancelLabel: string;
  isPending?: boolean;
  isConfirmDisabled?: boolean;
  error?: unknown;
  errorDescription?: string;
  children?: ReactNode;
  onConfirm: () => void;
  onCancel: () => void;
};

const FOCUSABLE_SELECTOR =
  'button:not([disabled]), [href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';

/**
 * Accessible confirmation surface for state-changing actions (DESIGN.md §4 /
 * §6). Escape and backdrop dismissal are disabled while the command is in
 * flight so a double-submit is impossible.
 */
export function ConfirmDialog({
  isOpen,
  title,
  description,
  confirmLabel,
  pendingLabel,
  cancelLabel,
  isPending = false,
  isConfirmDisabled = false,
  error,
  errorDescription,
  children,
  onConfirm,
  onCancel,
}: Props) {
  const { t } = useI18n();
  const dialogRef = useRef<HTMLDivElement>(null);
  const cancelRef = useRef<HTMLButtonElement>(null);
  const onCancelRef = useRef(onCancel);
  const isPendingRef = useRef(isPending);
  const titleId = useId();
  const descriptionId = useId();

  useEffect(() => {
    onCancelRef.current = onCancel;
    isPendingRef.current = isPending;
  }, [isPending, onCancel]);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    const previouslyFocused = document.activeElement as HTMLElement | null;
    cancelRef.current?.focus();

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape' && !isPendingRef.current) {
        event.preventDefault();
        onCancelRef.current();
        return;
      }

      if (event.key !== 'Tab') {
        return;
      }

      const focusable = Array.from(
        dialogRef.current?.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTOR) ?? [],
      );

      if (focusable.length === 0) {
        return;
      }

      const first = focusable[0];
      const last = focusable[focusable.length - 1];
      const active = document.activeElement;

      if (event.shiftKey && active === first) {
        event.preventDefault();
        last?.focus();
      } else if (!event.shiftKey && active === last) {
        event.preventDefault();
        first?.focus();
      }
    };

    document.addEventListener('keydown', handleKeyDown);

    return () => {
      document.removeEventListener('keydown', handleKeyDown);
      previouslyFocused?.focus();
    };
  }, [isOpen]);

  if (!isOpen) {
    return null;
  }

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-charcoal/50 p-4"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget && !isPending) {
          onCancel();
        }
      }}
    >
      <div
        ref={dialogRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        aria-describedby={descriptionId}
        aria-busy={isPending}
        className={cn(
          'w-full max-w-lg rounded-lg border border-line bg-surface p-6 shadow-md',
          'dialog-enter',
        )}
      >
        <h2 id={titleId} className="text-lg font-semibold text-charcoal">
          {title}
        </h2>
        <p id={descriptionId} className="mt-2 text-sm text-charcoal-light">
          {description}
        </p>

        {error ? (
          <div className="mt-4">
            <ErrorState
              description={errorDescription ?? t('sales.confirm.error')}
              traceId={getTraceId(error)}
            />
          </div>
        ) : null}

        {children ? <div className="mt-4">{children}</div> : null}

        <div className="mt-6 flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
          <button
            ref={cancelRef}
            type="button"
            onClick={onCancel}
            disabled={isPending}
            className="inline-flex h-10 items-center justify-center rounded-md border border-line bg-surface px-4 text-sm font-medium text-charcoal transition-colors duration-300 hover:bg-surface-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {cancelLabel}
          </button>
          <Button onClick={onConfirm} disabled={isPending || isConfirmDisabled}>
            {isPending ? pendingLabel : confirmLabel}
          </Button>
        </div>
      </div>
    </div>
  );
}

function getTraceId(error: unknown): string | undefined {
  if (
    typeof error === 'object' &&
    error !== null &&
    'traceId' in error &&
    typeof (error as { traceId?: unknown }).traceId === 'string'
  ) {
    return (error as { traceId: string }).traceId;
  }

  return undefined;
}
