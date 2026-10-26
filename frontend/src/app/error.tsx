'use client';

import { useEffect } from 'react';

import { ErrorState } from '@/components/shared/ErrorState';

type Props = {
  error: Error & { digest?: string };
  reset: () => void;
};

export default function RootError({ error, reset }: Props) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <main className="flex min-h-dvh items-center justify-center bg-bg p-6">
      <ErrorState traceId={error.digest} onRetry={reset} />
    </main>
  );
}
