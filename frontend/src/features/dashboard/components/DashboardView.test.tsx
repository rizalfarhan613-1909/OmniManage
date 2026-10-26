import { render, screen } from '@testing-library/react';
import type { ReactNode } from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import type { ExecutiveDashboardData } from '@/features/dashboard/api/types';
import { API_ERROR_CODES, ApiRequestError } from '@/lib/api/errors';
import { createSession } from '@/test/fixtures';

import { DashboardView } from './DashboardView';

const api = vi.hoisted(() => ({
  useSession: vi.fn(),
  useExecutiveDashboard: vi.fn(),
}));

vi.mock('@/components/providers/SessionProvider', () => ({
  useSession: api.useSession,
}));

vi.mock('@/features/dashboard/api/use-executive-dashboard', () => ({
  useExecutiveDashboard: api.useExecutiveDashboard,
}));

vi.mock('recharts', () => ({
  ResponsiveContainer: ({ children }: { children: ReactNode }) => <div>{children}</div>,
  AreaChart: ({ children }: { children: ReactNode }) => <svg>{children}</svg>,
  Area: () => null,
  CartesianGrid: () => null,
  XAxis: () => null,
  YAxis: () => null,
  Tooltip: () => null,
}));

const DATA: ExecutiveDashboardData = {
  available: {
    sales: true,
    inventory: true,
    stockValue: true,
    financeAr: true,
    financeGl: true,
    masterData: true,
  },
  revenueMtd: '5000000',
  revenuePreviousMonth: '4000000',
  revenueChangePercent: '25.0',
  openAr: '2000000',
  cashCollected: '1000000',
  stockValue: '3000000',
  revenueSeries: [
    {
      monthKey: '2026-10',
      label: 'Oct',
      revenue: '5000000',
      cashFlow: '1000000',
      revenueValue: 5_000_000,
      cashFlowValue: 1_000_000,
    },
  ],
  activity: [
    {
      id: 'activity-1',
      kind: 'salesConfirmed',
      reference: 'SO-001',
      amount: '1500000',
      occurredAt: '2026-10-20T10:00:00Z',
    },
  ],
  counts: {
    salesOrders: 3,
    arInvoices: 2,
    journalEntries: 4,
    stockBalances: 5,
    items: 6,
    warehouses: 1,
  },
};

function dashboardResult(overrides: Record<string, unknown> = {}) {
  return {
    data: DATA,
    isLoading: false,
    error: null,
    refetch: vi.fn(),
    isEnabled: true,
    scope: ['sales', 'inventory', 'ar', 'gl', 'masterdata'],
    ...overrides,
  };
}

beforeEach(() => {
  api.useSession.mockReturnValue(
    createSession({
      activeModules: [
        MODULE_CODES.salesCore,
        MODULE_CODES.invCore,
        MODULE_CODES.finAr,
        MODULE_CODES.finGl,
        MODULE_CODES.coreMasterData,
      ],
      permissions: [
        PERMISSIONS.sales.orderRead,
        PERMISSIONS.inventory.stockRead,
        PERMISSIONS.finance.invoiceRead,
        PERMISSIONS.finance.journalRead,
        PERMISSIONS.masterData.itemRead,
      ],
    }),
  );
  api.useExecutiveDashboard.mockReturnValue(dashboardResult());
});

describe('DashboardView', () => {
  it('renders KPI cards, trend, chart mirror table, activity and module usage', () => {
    render(<DashboardView />);

    expect(screen.getByText('Revenue MTD')).toBeInTheDocument();
    expect(screen.getAllByText('IDR 5,000,000.00')).toHaveLength(2);
    expect(screen.getByText('25.0% vs last month')).toBeInTheDocument();
    expect(screen.getByText('Open A/R')).toBeInTheDocument();
    expect(screen.getByText('IDR 2,000,000.00')).toBeInTheDocument();
    expect(screen.getByText('Stock value')).toBeInTheDocument();
    expect(screen.getByText('IDR 3,000,000.00')).toBeInTheDocument();
    expect(screen.getByText('Cash collected')).toBeInTheDocument();
    expect(screen.getAllByText('IDR 1,000,000.00')).toHaveLength(2);

    expect(
      screen.getByRole('table', { name: 'Revenue and cash flow by month' }),
    ).toBeInTheDocument();
    expect(screen.getByText('Order SO-001 confirmed')).toBeInTheDocument();
    expect(screen.getByText('SALES_CORE')).toBeInTheDocument();
    expect(screen.getByText('3 records')).toBeInTheDocument();
  });

  it('renders the dashboard skeleton while queries are in flight', () => {
    api.useExecutiveDashboard.mockReturnValue(dashboardResult({ isLoading: true }));

    render(<DashboardView />);

    expect(screen.getAllByRole('status').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Loading content').length).toBeGreaterThan(0);
  });

  it('renders the no-licensed-modules state when every gate is closed', () => {
    api.useExecutiveDashboard.mockReturnValue(dashboardResult({ isEnabled: false }));

    render(<DashboardView />);

    expect(screen.getByText('No licensed data modules')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Refresh dashboard' })).toBeInTheDocument();
  });

  it('renders the empty state when licensed modules have no records', () => {
    api.useExecutiveDashboard.mockReturnValue(
      dashboardResult({
        data: {
          ...DATA,
          revenueSeries: [],
          activity: [],
          counts: {
            salesOrders: 0,
            arInvoices: 0,
            journalEntries: 0,
            stockBalances: 0,
            items: 0,
            warehouses: 0,
          },
        },
      }),
    );

    render(<DashboardView />);

    expect(screen.getByText('No licensed data modules')).toBeInTheDocument();
  });

  it('renders the error state with the traceId and retry', () => {
    const refetch = vi.fn();
    api.useExecutiveDashboard.mockReturnValue(
      dashboardResult({
        data: undefined,
        error: new ApiRequestError({
          code: API_ERROR_CODES.unexpected,
          message: 'Dashboard aggregation failed.',
          status: 500,
          traceId: 'trace-dashboard-500',
        }),
        refetch,
      }),
    );

    render(<DashboardView />);

    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    expect(screen.getByText('trace-dashboard-500')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Try again' })).toBeInTheDocument();
  });

  it('renders forbidden when every active module rejects the read', () => {
    api.useExecutiveDashboard.mockReturnValue(
      dashboardResult({
        data: undefined,
        error: new ApiRequestError({
          code: API_ERROR_CODES.forbidden,
          message: 'Forbidden.',
          status: 403,
          traceId: 'trace-dashboard-403',
        }),
      }),
    );

    render(<DashboardView />);

    expect(screen.getByText("You don't have access")).toBeInTheDocument();
  });

  it('renders module-not-licensed when a licensed module is switched off server-side', () => {
    api.useExecutiveDashboard.mockReturnValue(
      dashboardResult({
        data: undefined,
        error: new ApiRequestError({
          code: API_ERROR_CODES.moduleNotLicensed,
          message: 'Module not licensed.',
          status: 403,
          traceId: 'trace-dashboard-licensed',
        }),
      }),
    );

    render(<DashboardView />);

    expect(screen.getByText('Module not active for your company')).toBeInTheDocument();
  });

  it('omits KPIs for unavailable modules instead of showing zeros', () => {
    api.useExecutiveDashboard.mockReturnValue(
      dashboardResult({
        data: {
          ...DATA,
          available: { ...DATA.available, sales: false, stockValue: false },
        },
      }),
    );

    render(<DashboardView />);

    expect(screen.queryByText('Revenue MTD')).not.toBeInTheDocument();
    expect(screen.queryByText('Stock value')).not.toBeInTheDocument();
    expect(screen.getByText('Open A/R')).toBeInTheDocument();
  });
});
