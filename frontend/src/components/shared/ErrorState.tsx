'use client';

import { AlertTriangle, Check, Copy, RotateCw } from 'lucide-react';
import { useState } from 'react';

import { Button } from '@/components/ui/Button';
import { t } from '@/lib/i18n';

type Props = {
  title?: string;
  description?: string;
  traceId?: string;
  onRetry?: () => void;
};

/** Required error state with retry and a copyable traceId (DESIGN.md §7). */
export function ErrorState({
  title = t('states.error.title'),
  description = t('states.error.body'),
  traceId,
  onRetry,
}: Props) {
  const [hasCopied, setHasCopied] = useState(false);

  const handleCopy = async () => {
    if (!traceId) {
      return;
    }

    try {
      await navigator.clipboard.writeText(traceId);
      setHasCopied(true);
    } catch (error) {
      console.warn('Trace ID could not be copied.', error);
    }
  };

  return (
    <section
      aria-labelledby="error-state-title"
      className="mx-auto flex max-w-xl flex-col items-center rounded-lg border border-sage bg-surface px-6 py-12 text-center shadow-sm"
    >
      <span className="bg-danger/10 flex h-12 w-12 items-center justify-center rounded-full">
        <AlertTriangle className="h-6 w-6 text-danger" aria-hidden="true" />
      </span>
      <h1 id="error-state-title" className="mt-4 text-lg font-semibold text-charcoal">
        {title}
      </h1>
      <p className="mt-2 text-sm text-charcoal-light">{description}</p>

      {traceId ? (
        <div className="mt-4 flex items-center gap-2 rounded-md bg-sage-light px-3 py-1.5">
          <code className="break-all font-mono text-xs text-charcoal">{traceId}</code>
          <Button
            variant="ghost"
            size="sm"
            onClick={handleCopy}
            className="h-7 px-2"
            aria-label={t('states.error.copyTraceId')}
          >
            {hasCopied ? (
              <Check className="h-4 w-4" aria-hidden="true" />
            ) : (
              <Copy className="h-4 w-4" aria-hidden="true" />
            )}
          </Button>
          <span aria-live="polite" className="sr-only">
            {hasCopied ? t('states.error.traceIdCopied') : ''}
          </span>
        </div>
      ) : null}

      {onRetry ? (
        <Button variant="primary" onClick={onRetry} className="mt-6">
          <RotateCw className="h-4 w-4" aria-hidden="true" />
          {t('states.error.retry')}
        </Button>
      ) : null}
    </section>
  );
}
