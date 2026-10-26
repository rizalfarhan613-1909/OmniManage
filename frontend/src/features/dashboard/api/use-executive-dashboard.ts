'use client';

import { useMemo } from 'react';

import { useSession } from '@/components/providers/SessionProvider';
import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import { useItems } from '@/features/masterdata/api/use-items';
import { useStockBalances } from '@/features/inventory/api/use-stock-balances';
import { useWarehouses } from '@/features/inventory/api/use-warehouses';
import { useArInvoices } from '@/features/finance/api/use-ar-invoices';
import { useJournalEntries } from '@/features/finance/api/use-journal-entries';
import { useSalesOrders } from '@/features/sales/api/use-sales-orders';
import { MAX_PAGE_SIZE } from '@/lib/api/pagination';
import {
  multiplyDecimals,
  percentageChange,
  sumDecimals,
  toChartNumber,
  toDecimal,
} from '@/lib/utils/decimal';
import { formatMonthKey } from '@/lib/utils/format';

import type {
  ActivityItem,
  DashboardAvailability,
  ExecutiveDashboardData,
  RevenuePoint,
} from './types';

const SERIES_MONTHS = 6;
const ACTIVITY_LIMIT = 8;

function monthKeyOf(isoDate: string): string {
  const date = new Date(isoDate);

  if (Number.isNaN(date.getTime())) {
    return '';
  }

  return `${date.getUTCFullYear()}-${String(date.getUTCMonth() + 1).padStart(2, '0')}`;
}

function lastMonthKeys(count: number): string[] {
  const now = new Date();
  const keys: string[] = [];

  for (let offset = count - 1; offset >= 0; offset -= 1) {
    const date = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth() - offset, 1));
    keys.push(monthKeyOf(date.toISOString()));
  }

  return keys;
}

function addAmount(map: Map<string, string>, key: string, amount: string): void {
  const current = map.get(key) ?? '0';
  map.set(key, toDecimal(current).plus(toDecimal(amount)).toString());
}

/**
 * Executive dashboard aggregation.
 *
 * The Phase 1-3 backend exposes no dashboard read-model endpoint yet, so this
 * hook composes the documented list endpoints (Rules.md §0.3 forbids inventing
 * endpoints). Queries are gated by BOTH an active license and a read
 * permission, so an OFF module never fires a request (DESIGN.md §3.3).
 *
 * ASSUMPTIONS:
 *  - "Cash flow" is collections from Paid A/R invoices. The DTO has no paidAt,
 *    so createdAt is used as the collection timestamp.
 *  - Aggregates cover the newest `MAX_PAGE_SIZE` (100) records per module. The
 *    projection read model promised by Architecture.md §9 replaces this when
 *    it lands; the hook signature stays unchanged.
 */
export function useExecutiveDashboard() {
  const session = useSession();
  const activeModules = session.activeModules;
  const permissions = session.permissions;

  const available = useMemo<DashboardAvailability>(() => {
    const hasModule = (moduleCode: string) => activeModules.includes(moduleCode);
    const hasPermission = (permission: string) => permissions.includes(permission);

    return {
      sales: hasModule(MODULE_CODES.salesCore) && hasPermission(PERMISSIONS.sales.orderRead),
      inventory: hasModule(MODULE_CODES.invCore) && hasPermission(PERMISSIONS.inventory.stockRead),
      stockValue:
        hasModule(MODULE_CODES.invCore) &&
        hasPermission(PERMISSIONS.inventory.stockRead) &&
        hasModule(MODULE_CODES.coreMasterData) &&
        hasPermission(PERMISSIONS.masterData.itemRead),
      financeAr: hasModule(MODULE_CODES.finAr) && hasPermission(PERMISSIONS.finance.invoiceRead),
      financeGl: hasModule(MODULE_CODES.finGl) && hasPermission(PERMISSIONS.finance.journalRead),
      masterData:
        hasModule(MODULE_CODES.coreMasterData) && hasPermission(PERMISSIONS.masterData.itemRead),
    };
  }, [activeModules, permissions]);

  const scope = [
    available.sales ? 'sales' : '',
    available.inventory ? 'inventory' : '',
    available.financeAr ? 'ar' : '',
    available.financeGl ? 'gl' : '',
    available.masterData ? 'masterdata' : '',
  ].filter((value) => value.length > 0);

  const listParams = { page: 1, pageSize: MAX_PAGE_SIZE };

  const ordersQuery = useSalesOrders(
    { ...listParams, sort: '-createdAt' },
    { enabled: available.sales },
  );
  const invoicesQuery = useArInvoices(
    { ...listParams, sort: '-createdAt' },
    { enabled: available.financeAr },
  );
  const journalsQuery = useJournalEntries(
    { ...listParams, sort: '-postedDateUtc' },
    { enabled: available.financeGl },
  );
  const balancesQuery = useStockBalances(listParams, { enabled: available.inventory });
  const itemsQuery = useItems(listParams, { enabled: available.masterData });
  const warehousesQuery = useWarehouses(listParams, {
    enabled: available.inventory && permissions.includes(PERMISSIONS.inventory.warehouseRead),
  });

  const orderItems = ordersQuery.data?.items;
  const invoiceItems = invoicesQuery.data?.items;
  const journalItems = journalsQuery.data?.items;
  const balanceItems = balancesQuery.data?.items;
  const itemItems = itemsQuery.data?.items;

  const activeQueries = [
    { enabled: available.sales, query: ordersQuery },
    { enabled: available.inventory, query: balancesQuery },
    { enabled: available.masterData, query: itemsQuery },
    { enabled: available.financeAr, query: invoicesQuery },
    { enabled: available.financeGl, query: journalsQuery },
    {
      enabled: available.inventory && permissions.includes(PERMISSIONS.inventory.warehouseRead),
      query: warehousesQuery,
    },
  ].filter((entry) => entry.enabled);

  const isLoading = activeQueries.some((entry) => entry.query.isLoading);
  const error = activeQueries.find((entry) => entry.query.error)?.query.error ?? null;

  const refetch = () => {
    void Promise.all(activeQueries.map((entry) => entry.query.refetch()));
  };

  const data = useMemo<ExecutiveDashboardData>(() => {
    const orders = orderItems ?? [];
    const invoices = invoiceItems ?? [];
    const journals = journalItems ?? [];
    const balances = balanceItems ?? [];
    const items = itemItems ?? [];

    const months = lastMonthKeys(SERIES_MONTHS);
    const currentMonth = months[months.length - 1] ?? '';
    const previousMonth = months[months.length - 2] ?? '';
    const revenueByMonth = new Map<string, string>();
    const cashByMonth = new Map<string, string>();

    for (const order of orders) {
      if (order.status !== 'Confirmed') {
        continue;
      }

      const key = monthKeyOf(order.createdAt);

      if (key) {
        addAmount(revenueByMonth, key, order.totalAmount);
      }
    }

    for (const invoice of invoices) {
      if (invoice.status !== 'Paid') {
        continue;
      }

      const key = monthKeyOf(invoice.createdAt);

      if (key) {
        addAmount(cashByMonth, key, invoice.totalAmount);
      }
    }

    const revenueMtd = revenueByMonth.get(currentMonth) ?? '0';
    const revenuePreviousMonth = revenueByMonth.get(previousMonth) ?? '0';
    const change = percentageChange(revenueMtd, revenuePreviousMonth);

    const openAr = sumDecimals(
      invoices
        .filter((invoice) => invoice.status === 'Unpaid')
        .map((invoice) => invoice.totalAmount),
    ).toString();

    const cashCollected = cashByMonth.get(currentMonth) ?? '0';

    const itemsById = new Map(items.map((item) => [item.id, item]));
    const stockValue = sumDecimals(
      balances.map((balance) => {
        const item = itemsById.get(balance.itemId);

        return item ? multiplyDecimals(balance.quantityOnHand, item.costPrice) : '0';
      }),
    ).toString();

    const revenueSeries: RevenuePoint[] = months.map((monthKey) => {
      const revenue = revenueByMonth.get(monthKey) ?? '0';
      const cashFlow = cashByMonth.get(monthKey) ?? '0';

      return {
        monthKey,
        label: formatMonthKey(monthKey),
        revenue,
        cashFlow,
        revenueValue: toChartNumber(revenue),
        cashFlowValue: toChartNumber(cashFlow),
      };
    });

    const activity: ActivityItem[] = [
      ...orders
        .filter((order) => order.status !== 'Cancelled')
        .map<ActivityItem>((order) => ({
          id: `order-${order.id}`,
          kind: order.status === 'Confirmed' ? 'salesConfirmed' : 'salesDraft',
          reference: order.orderNumber,
          amount: order.totalAmount,
          occurredAt: order.updatedAt ?? order.createdAt,
        })),
      ...invoices.map<ActivityItem>((invoice) => ({
        id: `invoice-${invoice.id}`,
        kind: invoice.status === 'Paid' ? 'invoicePaid' : 'invoiceUnpaid',
        reference: invoice.invoiceNumber,
        amount: invoice.totalAmount,
        occurredAt: invoice.createdAt,
      })),
      ...journals.map<ActivityItem>((journal) => ({
        id: `journal-${journal.id}`,
        kind: 'journalPosted',
        reference: journal.entryNumber,
        amount: journal.debitTotal,
        occurredAt: journal.postedDateUtc,
      })),
    ]
      .sort(
        (left, right) => new Date(right.occurredAt).getTime() - new Date(left.occurredAt).getTime(),
      )
      .slice(0, ACTIVITY_LIMIT);

    return {
      available,
      revenueMtd,
      revenuePreviousMonth,
      revenueChangePercent: change ? change.toFixed(1) : null,
      openAr,
      cashCollected,
      stockValue,
      revenueSeries,
      activity,
      counts: {
        salesOrders: ordersQuery.data?.totalItems ?? 0,
        arInvoices: invoicesQuery.data?.totalItems ?? 0,
        journalEntries: journalsQuery.data?.totalItems ?? 0,
        stockBalances: balancesQuery.data?.totalItems ?? 0,
        items: itemsQuery.data?.totalItems ?? 0,
        warehouses: warehousesQuery.data?.totalItems ?? 0,
      },
    };
  }, [
    orderItems,
    invoiceItems,
    journalItems,
    balanceItems,
    itemItems,
    ordersQuery.data?.totalItems,
    invoicesQuery.data?.totalItems,
    journalsQuery.data?.totalItems,
    balancesQuery.data?.totalItems,
    itemsQuery.data?.totalItems,
    warehousesQuery.data?.totalItems,
    available,
  ]);

  return {
    data,
    isLoading,
    error,
    refetch,
    isEnabled: activeQueries.length > 0,
    scope,
  };
}
