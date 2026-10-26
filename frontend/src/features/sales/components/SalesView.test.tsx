import { fireEvent, render, screen, within } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import type { SalesOrder } from '@/features/sales/api/types';
import { API_ERROR_CODES, ApiRequestError } from '@/lib/api/errors';
import { createSession, pageOf } from '@/test/fixtures';
import { mutationResult, queryResult } from '@/test/query';

import { SalesView } from './SalesView';

const api = vi.hoisted(() => ({
  useSession: vi.fn(),
  useSalesOrders: vi.fn(),
  useConfirmSalesOrder: vi.fn(),
  useBusinessPartners: vi.fn(),
}));

vi.mock('@/components/providers/SessionProvider', () => ({
  useSession: api.useSession,
}));

vi.mock('@/features/sales/api/use-sales-orders', () => ({
  useSalesOrders: api.useSalesOrders,
}));

vi.mock('@/features/sales/api/use-confirm-sales-order', () => ({
  useConfirmSalesOrder: api.useConfirmSalesOrder,
}));

vi.mock('@/features/masterdata/api/use-business-partners', () => ({
  useBusinessPartners: api.useBusinessPartners,
}));

const DRAFT: SalesOrder = {
  id: 'so-1',
  orderNumber: 'SO-001',
  customerId: 'bp-1',
  status: 'Draft',
  totalAmount: '1500000',
  lines: [],
  createdAt: '2026-10-20T10:00:00Z',
  updatedAt: null,
};

const CONFIRMED: SalesOrder = {
  id: 'so-2',
  orderNumber: 'SO-002',
  customerId: 'bp-1',
  status: 'Confirmed',
  totalAmount: '2500000',
  lines: [],
  createdAt: '2026-10-19T10:00:00Z',
  updatedAt: '2026-10-19T11:00:00Z',
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
      activeModules: [MODULE_CODES.salesCore, MODULE_CODES.coreMasterData],
      permissions: [
        PERMISSIONS.sales.orderRead,
        PERMISSIONS.sales.orderConfirm,
        PERMISSIONS.masterData.businessPartnerRead,
      ],
    }),
  );
  api.useBusinessPartners.mockReturnValue(queryResult({ data: PARTNERS }));
  api.useSalesOrders.mockReturnValue(queryResult({ data: pageOf([DRAFT, CONFIRMED]) }));
  api.useConfirmSalesOrder.mockReturnValue(mutationResult());
});

describe('SalesView', () => {
  it('renders orders with customer names, status badges and formatted totals', () => {
    render(<SalesView />);

    expect(screen.getByText('SO-001')).toBeInTheDocument();
    expect(screen.getByText('SO-002')).toBeInTheDocument();
    expect(screen.getAllByText('Acme Retail')).toHaveLength(2);
    expect(screen.getByText('Draft')).toBeInTheDocument();
    expect(screen.getByText('Confirmed')).toBeInTheDocument();
    expect(screen.getByText('IDR 1,500,000.00')).toBeInTheDocument();
    expect(screen.getByText('IDR 2,500,000.00')).toBeInTheDocument();
  });

  it('only offers confirmation for drafts when the permission is present', () => {
    render(<SalesView />);

    expect(screen.getAllByRole('button', { name: 'Confirm order' })).toHaveLength(1);
  });

  it('hides confirmation without the UC-01 permission', () => {
    api.useSession.mockReturnValue(
      createSession({
        activeModules: [MODULE_CODES.salesCore],
        permissions: [PERMISSIONS.sales.orderRead],
      }),
    );

    render(<SalesView />);

    expect(screen.queryByRole('button', { name: 'Confirm order' })).not.toBeInTheDocument();
  });

  it('confirms a draft through the dialog and announces success', () => {
    const mutate = vi.fn(
      (orderId: string, options?: { onSuccess?: (order: SalesOrder) => void }) => {
        expect(orderId).toBe('so-1');
        options?.onSuccess?.({ ...DRAFT, status: 'Confirmed' });
      },
    );
    api.useConfirmSalesOrder.mockReturnValue(mutationResult({ mutate }));

    render(<SalesView />);

    fireEvent.click(screen.getByRole('button', { name: 'Confirm order' }));

    const dialog = screen.getByRole('dialog');
    expect(
      within(dialog).getByRole('heading', { name: 'Confirm order SO-001?' }),
    ).toBeInTheDocument();

    fireEvent.click(within(dialog).getByRole('button', { name: 'Confirm order' }));

    expect(mutate).toHaveBeenCalledTimes(1);
    expect(
      screen.getByText('Order SO-001 confirmed. Stock and A/R events were queued.'),
    ).toBeInTheDocument();
  });

  it('renders the loading skeleton while the order list loads', () => {
    api.useSalesOrders.mockReturnValue(queryResult({ isLoading: true }));

    render(<SalesView />);

    expect(screen.getByRole('status')).toHaveTextContent('Loading content');
  });

  it('renders the empty state with a teal refresh CTA', () => {
    api.useSalesOrders.mockReturnValue(queryResult({ data: pageOf([]) }));

    render(<SalesView />);

    expect(screen.getByText('No sales orders yet')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Refresh orders' })).toBeInTheDocument();
  });

  it('renders the error state with the traceId', () => {
    api.useSalesOrders.mockReturnValue(
      queryResult({
        error: new ApiRequestError({
          code: API_ERROR_CODES.unexpected,
          message: 'The order list could not be loaded.',
          status: 500,
          traceId: 'trace-sales-500',
        }),
      }),
    );

    render(<SalesView />);

    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    expect(screen.getByText('trace-sales-500')).toBeInTheDocument();
  });

  it('renders forbidden when the API rejects the read permission', () => {
    api.useSalesOrders.mockReturnValue(
      queryResult({
        error: new ApiRequestError({
          code: API_ERROR_CODES.forbidden,
          message: 'Forbidden.',
          status: 403,
          traceId: 'trace-sales-403',
        }),
      }),
    );

    render(<SalesView />);

    expect(screen.getByText("You don't have access")).toBeInTheDocument();
  });

  it('renders module-not-licensed when Sales is not active for the tenant', () => {
    api.useSalesOrders.mockReturnValue(
      queryResult({
        error: new ApiRequestError({
          code: API_ERROR_CODES.moduleNotLicensed,
          message: 'Module not licensed.',
          status: 403,
          traceId: 'trace-sales-licensed',
        }),
      }),
    );

    render(<SalesView />);

    expect(screen.getByText('Module not active for your company')).toBeInTheDocument();
  });
});
