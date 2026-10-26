'use client';

import { useEffect } from 'react';

import { ErrorState } from '@/components/shared/ErrorState';

type Props = {
  error: Error & { digest?: string };
  reset: () => void;
};

/** Route-segment error boundary required by Rules.md §6.8. */
export default function AppError({ error, reset }: Props) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return <ErrorState traceId={error.digest} onRetry={reset} />;
}
