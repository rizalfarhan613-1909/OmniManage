'use client';

import { useI18n } from '@/components/providers/LanguageProvider';

import { CheckCircle2 } from 'lucide-react';
import { useMemo, useState } from 'react';

import { useSession } from '@/components/providers/SessionProvider';
import { DataTable, type DataTableColumn } from '@/components/shared/DataTable';
import { PageHeader } from '@/components/shared/PageHeader';
import { QuerySection } from '@/components/shared/QuerySection';
import { TableSkeleton } from '@/components/shared/Skeletons';
import { StatusBadge, type StatusTone } from '@/components/shared/StatusBadge';
import { Button } from '@/components/ui/Button';
import { PERMISSIONS } from '@/config/permissions';
import { useBusinessPartners } from '@/features/masterdata/api/use-business-partners';
import { useConfirmSalesOrder } from '@/features/sales/api/use-confirm-sales-order';
import { useSalesOrders } from '@/features/sales/api/use-sales-orders';
import type { SalesOrder, SalesOrderStatus } from '@/features/sales/api/types';
import { MAX_PAGE_SIZE, toSortParam, type SortState } from '@/lib/api/pagination';

import { formatDateTime, formatMoney } from '@/lib/utils/format';

import { ConfirmOrderDialog } from './ConfirmOrderDialog';

const PAGE_SIZE = 10;

const STATUS_TONE: Record<SalesOrderStatus, StatusTone> = {
  Draft: 'warning',
  Confirmed: 'success',
  Cancelled: 'neutral',
};

const STATUS_LABEL = {
  Draft: 'sales.status.draft',
  Confirmed: 'sales.status.confirmed',
  Cancelled: 'sales.status.cancelled',
} as const;

export function SalesView() {
  const { t } = useI18n();
  const session = useSession();
  const [page, setPage] = useState(1);
  const [sort, setSort] = useState<SortState>({ field: 'createdAt', direction: 'desc' });
  const [selectedOrder, setSelectedOrder] = useState<SalesOrder | null>(null);
  const [lastConfirmed, setLastConfirmed] = useState<string | null>(null);

  const canConfirm = session.permissions.includes(PERMISSIONS.sales.orderConfirm);
  const canReadPartners = session.permissions.includes(PERMISSIONS.masterData.businessPartnerRead);

  const ordersQuery = useSalesOrders({
    page,
    pageSize: PAGE_SIZE,
    sort: toSortParam(sort),
  });
  const partnersQuery = useBusinessPartners(
    { page: 1, pageSize: MAX_PAGE_SIZE },
    { enabled: canReadPartners },
  );
  const confirmMutation = useConfirmSalesOrder();

  const partnerNames = useMemo(() => {
    const map = new Map<string, string>();

    for (const partner of partnersQuery.data?.items ?? []) {
      map.set(partner.id, partner.name);
    }

    return map;
  }, [partnersQuery.data]);

  const handleOpenConfirm = (order: SalesOrder) => {
    confirmMutation.reset();
    setSelectedOrder(order);
  };

  const handleCancelConfirm = () => {
    if (!confirmMutation.isPending) {
      setSelectedOrder(null);
    }
  };

  const handleConfirm = () => {
    if (!selectedOrder) {
      return;
    }

    confirmMutation.mutate(selectedOrder.id, {
      onSuccess: (order) => {
        setLastConfirmed(order.orderNumber);
        setSelectedOrder(null);
      },
    });
  };

  const columns: DataTableColumn<SalesOrder>[] = [
    {
      id: 'orderNumber',
      header: t('sales.orders.number'),
      sortable: true,
      cell: (order) => (
        <span className="font-mono text-xs font-semibold text-charcoal">{order.orderNumber}</span>
      ),
    },
    {
      id: 'customer',
      header: t('sales.orders.customer'),
      cell: (order) => (
        <span className="text-sm text-charcoal">
          {partnerNames.get(order.customerId) ?? t('sales.orders.unknownCustomer')}
        </span>
      ),
    },
    {
      id: 'status',
      header: t('sales.orders.status'),
      sortable: true,
      cell: (order) => (
        <StatusBadge label={t(STATUS_LABEL[order.status])} tone={STATUS_TONE[order.status]} />
      ),
    },
    {
      id: 'totalAmount',
      header: t('sales.orders.total'),
      sortable: true,
      align: 'right',
      className: 'tabular-nums',
      cell: (order) => formatMoney(order.totalAmount),
    },
    {
      id: 'createdAt',
      header: t('sales.orders.createdAt'),
      sortable: true,
      cell: (order) => (
        <span className="whitespace-nowrap text-charcoal-light">
          {formatDateTime(order.createdAt)}
        </span>
      ),
    },
    {
      id: 'actions',
      header: t('sales.orders.actions'),
      align: 'right',
      cell: (order) =>
        order.status === 'Draft' && canConfirm ? (
          <Button size="sm" variant="secondary" onClick={() => handleOpenConfirm(order)}>
            {t('sales.confirm.action')}
          </Button>
        ) : (
          <span aria-hidden="true" className="text-charcoal-light">
            &mdash;
          </span>
        ),
    },
  ];

  return (
    <div className="space-y-6">
      <PageHeader title={t('sales.orders.title')} description={t('sales.orders.description')} />

      {lastConfirmed ? (
        <div
          role="status"
          aria-live="polite"
          className="flex items-start gap-2 rounded-lg border border-mint-accent bg-mint-accent/30 px-4 py-3 text-sm text-charcoal"
        >
          <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0" aria-hidden="true" />
          <p>{t('sales.orders.confirmSuccess', { orderNumber: lastConfirmed })}</p>
        </div>
      ) : null}

      <QuerySection
        query={ordersQuery}
        skeleton={<TableSkeleton columns={columns.length} rows={7} />}
        isEmpty={(data) => data.items.length === 0}
        emptyTitle={t('sales.orders.empty.title')}
        emptyDescription={t('sales.orders.empty.body')}
        emptyActionLabel={t('sales.orders.empty.cta')}
      >
        {(data) => (
          <DataTable
            data={data.items}
            columns={columns}
            getRowId={(order) => order.id}
            caption={t('sales.orders.tableCaption')}
            sort={sort}
            onSortChange={(next) => {
              setSort(next);
              setPage(1);
            }}
            pagination={{
              page: data.page,
              pageSize: data.pageSize,
              totalItems: data.totalItems,
              totalPages: data.totalPages,
            }}
            onPageChange={setPage}
          />
        )}
      </QuerySection>

      <ConfirmOrderDialog
        isOpen={selectedOrder !== null}
        order={selectedOrder}
        isPending={confirmMutation.isPending}
        error={confirmMutation.error}
        onConfirm={handleConfirm}
        onCancel={handleCancelConfirm}
      />
    </div>
  );
}
