import { render, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import type { ArInvoice, JournalEntry } from '@/features/finance/api/types';
import { API_ERROR_CODES, ApiRequestError } from '@/lib/api/errors';
import { createSession, pageOf } from '@/test/fixtures';
import { queryResult } from '@/test/query';

import { FinanceView } from './FinanceView';

const api = vi.hoisted(() => ({
  useSession: vi.fn(),
  useArInvoices: vi.fn(),
  useJournalEntries: vi.fn(),
  useBusinessPartners: vi.fn(),
}));

vi.mock('@/components/providers/SessionProvider', () => ({
  useSession: api.useSession,
}));

vi.mock('@/features/finance/api/use-ar-invoices', () => ({
  useArInvoices: api.useArInvoices,
}));

vi.mock('@/features/finance/api/use-journal-entries', () => ({
  useJournalEntries: api.useJournalEntries,
}));

vi.mock('@/features/masterdata/api/use-business-partners', () => ({
  useBusinessPartners: api.useBusinessPartners,
}));

const INVOICE: ArInvoice = {
  id: 'inv-1',
  invoiceNumber: 'INV-001',
  salesOrderId: 'so-1',
  customerId: 'bp-1',
  totalAmount: '1500000',
  status: 'Unpaid',
  createdAt: '2026-10-20T10:00:00Z',
};

const JOURNAL: JournalEntry = {
  id: 'je-1',
  entryNumber: 'JE-001',
  postedDateUtc: '2026-10-20T10:05:00Z',
  description: 'Sales order confirmation',
  debitTotal: '1500000',
  creditTotal: '1500000',
  lines: [
    { glAccountId: 'ar', debit: '1500000', credit: '0' },
    { glAccountId: 'revenue', debit: '0', credit: '1500000' },
  ],
  createdAt: '2026-10-20T10:05:00Z',
};

const PARTNERS = pageOf([
  {
    id: 'bp-1',
    code: 'CUST-1',
    name: 'Acme Retail',
    partnerType: 'Customer' as const,
    taxIdMasked: null,
    creditLimit: '0',
    isActive: true,
    createdAt: '2026-01-01T00:00:00Z',
  },
]);

beforeEach(() => {
  api.useSession.mockReturnValue(
    createSession({
      activeModules: [MODULE_CODES.finAr, MODULE_CODES.finGl, MODULE_CODES.coreMasterData],
      permissions: [
        PERMISSIONS.finance.invoiceRead,
        PERMISSIONS.finance.journalRead,
        PERMISSIONS.masterData.businessPartnerRead,
      ],
    }),
  );
  api.useArInvoices.mockReturnValue(queryResult({ data: pageOf([INVOICE]) }));
  api.useJournalEntries.mockReturnValue(queryResult({ data: pageOf([JOURNAL]) }));
  api.useBusinessPartners.mockReturnValue(queryResult({ data: PARTNERS }));
});

describe('FinanceView', () => {
  it('renders A/R invoices alongside the journal ledger', () => {
    render(<FinanceView />);

    expect(screen.getByText('INV-001')).toBeInTheDocument();
    expect(screen.getByText('Acme Retail')).toBeInTheDocument();
    expect(screen.getByText('Unpaid')).toBeInTheDocument();
    expect(screen.getAllByText('IDR 1,500,000.00')).toHaveLength(3);

    expect(screen.getByText('JE-001')).toBeInTheDocument();
    expect(screen.getByText('Sales order confirmation')).toBeInTheDocument();
    expect(screen.getByText('2')).toBeInTheDocument();
  });

  it('renders module-not-licensed for A/R without blocking the ledger', () => {
    api.useSession.mockReturnValue(
      createSession({
        activeModules: [MODULE_CODES.finGl],
        permissions: [PERMISSIONS.finance.journalRead],
      }),
    );

    render(<FinanceView />);

    expect(screen.getByText('Module not active for your company')).toBeInTheDocument();
    expect(screen.getByText('Module: FIN_AR')).toBeInTheDocument();
    expect(screen.getByText('JE-001')).toBeInTheDocument();
  });

  it('renders forbidden for A/R when the invoice read permission is missing', () => {
    api.useSession.mockReturnValue(
      createSession({
        activeModules: [MODULE_CODES.finAr, MODULE_CODES.finGl],
        permissions: [PERMISSIONS.finance.journalRead],
      }),
    );

    render(<FinanceView />);

    expect(screen.getByText("You don't have access")).toBeInTheDocument();
    expect(screen.getByText('JE-001')).toBeInTheDocument();
  });

  it('renders the empty A/R state', () => {
    api.useArInvoices.mockReturnValue(queryResult({ data: pageOf([]) }));

    render(<FinanceView />);

    expect(screen.getByText('No A/R invoices yet')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Refresh invoices' })).toBeInTheDocument();
  });

  it('renders the journal loading skeleton independently', () => {
    api.useJournalEntries.mockReturnValue(queryResult({ isLoading: true }));

    render(<FinanceView />);

    expect(screen.getByRole('status')).toHaveTextContent('Loading content');
  });

  it('renders the journal error state with the traceId', () => {
    api.useJournalEntries.mockReturnValue(
      queryResult({
        error: new ApiRequestError({
          code: API_ERROR_CODES.unexpected,
          message: 'The ledger could not be loaded.',
          status: 500,
          traceId: 'trace-finance-500',
        }),
      }),
    );

    render(<FinanceView />);

    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    expect(screen.getByText('trace-finance-500')).toBeInTheDocument();
  });
});
