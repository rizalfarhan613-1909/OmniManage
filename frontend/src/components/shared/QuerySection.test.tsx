import { QueryClientProvider, useQuery } from '@tanstack/react-query';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import type { ReactNode } from 'react';
import { describe, expect, it, vi } from 'vitest';

import { API_ERROR_CODES, ApiRequestError } from '@/lib/api/errors';
import { createTestQueryClient } from '@/test/query';

import { QuerySection } from './QuerySection';

type HarnessProps = {
  queryFn: () => Promise<string[]>;
  moduleCode?: string;
};

function Harness({ queryFn, moduleCode }: HarnessProps) {
  const query = useQuery({
    queryKey: ['query-section-test', queryFn],
    queryFn,
    retry: false,
    gcTime: Infinity,
  });

  return (
    <QuerySection
      query={query}
      skeleton={<p>Loading skeleton</p>}
      isEmpty={(data) => data.length === 0}
      emptyTitle="Empty title"
      emptyDescription="Empty description"
      emptyActionLabel="Refresh data"
      moduleCode={moduleCode}
    >
      {(data) => <p>{data.join(', ')}</p>}
    </QuerySection>
  );
}

function renderSection(queryFn: () => Promise<string[]>, moduleCode?: string) {
  const queryClient = createTestQueryClient();

  function Wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
  }

  return render(<Harness queryFn={queryFn} moduleCode={moduleCode} />, { wrapper: Wrapper });
}

describe('QuerySection', () => {
  it('renders the sage skeleton while the query is loading', () => {
    renderSection(() => new Promise<string[]>(() => {}));

    expect(screen.getByText('Loading skeleton')).toBeInTheDocument();
  });

  it('renders the empty state with a refresh action', async () => {
    const queryFn = vi.fn<() => Promise<string[]>>().mockResolvedValue([]);

    renderSection(queryFn);

    expect(await screen.findByText('Empty title')).toBeInTheDocument();
    expect(screen.getByText('Empty description')).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Refresh data' }));

    await waitFor(() => expect(queryFn).toHaveBeenCalledTimes(2));
  });

  it('renders children when data is present', async () => {
    renderSection(vi.fn<() => Promise<string[]>>().mockResolvedValue(['Alpha', 'Beta']));

    expect(await screen.findByText('Alpha, Beta')).toBeInTheDocument();
  });

  it('renders the error state with a copyable traceId and retry', async () => {
    const error = new ApiRequestError({
      code: API_ERROR_CODES.unexpected,
      message: 'The order could not be loaded.',
      status: 500,
      traceId: 'trace-section-500',
    });
    const queryFn = vi.fn<() => Promise<string[]>>().mockRejectedValue(error);

    renderSection(queryFn);

    expect(await screen.findByText('Something went wrong')).toBeInTheDocument();
    expect(screen.getByText('The order could not be loaded.')).toBeInTheDocument();
    expect(screen.getByText('trace-section-500')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Copy trace ID' })).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Try again' }));

    await waitFor(() => expect(queryFn).toHaveBeenCalledTimes(2));
  });

  it('maps a permission failure to the forbidden state', async () => {
    renderSection(
      vi.fn<() => Promise<string[]>>().mockRejectedValue(
        new ApiRequestError({
          code: API_ERROR_CODES.forbidden,
          message: 'Forbidden.',
          status: 403,
          traceId: 'trace-forbidden',
        }),
      ),
    );

    expect(await screen.findByText("You don't have access")).toBeInTheDocument();
  });

  it('maps a licensing failure to the module-not-licensed state', async () => {
    renderSection(
      vi.fn<() => Promise<string[]>>().mockRejectedValue(
        new ApiRequestError({
          code: API_ERROR_CODES.moduleNotLicensed,
          message: 'Module not licensed.',
          status: 403,
          traceId: 'trace-licensed',
        }),
      ),
      'FIN_AR',
    );

    expect(await screen.findByText('Module not active for your company')).toBeInTheDocument();
    expect(screen.getByText('Module: FIN_AR')).toBeInTheDocument();
  });
});
