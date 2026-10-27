'use client';

import { useI18n } from '@/components/providers/LanguageProvider';

import { CheckCircle2, Plus } from 'lucide-react';
import { useState } from 'react';

import { useSession } from '@/components/providers/SessionProvider';
import { DataTable, type DataTableColumn } from '@/components/shared/DataTable';
import { PageHeader } from '@/components/shared/PageHeader';
import { QuerySection } from '@/components/shared/QuerySection';
import { TableSkeleton } from '@/components/shared/Skeletons';
import { StatusBadge, type StatusTone } from '@/components/shared/StatusBadge';
import { Button } from '@/components/ui/Button';
import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import { useItems } from '@/features/masterdata/api/use-items';
import type {
  CreatePurchaseOrderRequest,
  PurchaseOrder,
  PurchaseOrderStatus,
} from '@/features/procurement/api/types';
import { useCreatePurchaseOrder } from '@/features/procurement/api/useCreatePurchaseOrder';
import { useGetPurchaseOrders } from '@/features/procurement/api/useGetPurchaseOrders';
import { useGetSuppliers } from '@/features/procurement/api/useGetSuppliers';
import { MAX_PAGE_SIZE, toSortParam, type SortState } from '@/lib/api/pagination';

import { formatDate, formatDateTime, formatMoney } from '@/lib/utils/format';

import { CreatePurchaseOrderDialog } from './CreatePurchaseOrderDialog';

const PAGE_SIZE = 10;

const STATUS_TONE: Record<PurchaseOrderStatus, StatusTone> = {
  Draft: 'warning',
  Approved: 'info',
  PartiallyReceived: 'info',
  Received: 'success',
  Cancelled: 'neutral',
};

const STATUS_LABEL = {
  Draft: 'procurement.status.draft',
  Approved: 'procurement.status.approved',
  PartiallyReceived: 'procurement.status.partiallyReceived',
  Received: 'procurement.status.received',
  Cancelled: 'procurement.status.cancelled',
} as const;

export function PurchaseOrdersView() {
  const { t } = useI18n();
  const session = useSession();
  const [page, setPage] = useState(1);
  const [sort, setSort] = useState<SortState>({ field: 'createdAt', direction: 'desc' });
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [lastCreated, setLastCreated] = useState<string | null>(null);

  const canCreate = session.permissions.includes(PERMISSIONS.procurement.purchaseOrderCreate);
  const canReadSuppliers = session.permissions.includes(PERMISSIONS.procurement.supplierRead);
  const canReadItems =
    session.activeModules.includes(MODULE_CODES.coreMasterData) &&
    session.permissions.includes(PERMISSIONS.masterData.itemRead);

  const ordersQuery = useGetPurchaseOrders({
    page,
    pageSize: PAGE_SIZE,
    sort: toSortParam(sort),
  });
  const suppliersQuery = useGetSuppliers(
    { page: 1, pageSize: MAX_PAGE_SIZE, isActive: true },
    { enabled: canCreate && canReadSuppliers },
  );
  const itemsQuery = useItems(
    { page: 1, pageSize: MAX_PAGE_SIZE },
    { enabled: canCreate && canReadItems },
  );

  const createMutation = useCreatePurchaseOrder();

  const suppliers = suppliersQuery.data?.items ?? [];
  const items = itemsQuery.data?.items ?? [];

  const handleOpenCreate = () => {
    createMutation.reset();
    setIsCreateOpen(true);
  };

  const handleCloseCreate = () => {
    if (!createMutation.isPending) {
      setIsCreateOpen(false);
    }
  };

  const handleCreate = (request: CreatePurchaseOrderRequest) => {
    createMutation.mutate(request, {
      onSuccess: (order) => {
        setLastCreated(order.orderNumber);
        setIsCreateOpen(false);
      },
    });
  };

  const columns: DataTableColumn<PurchaseOrder>[] = [
    {
      id: 'orderNumber',
      header: t('procurement.orders.number'),
      sortable: true,
      cell: (order) => (
        <span className="font-mono text-xs font-semibold text-charcoal">{order.orderNumber}</span>
      ),
    },
    {
      id: 'supplierName',
      header: t('procurement.orders.supplier'),
      sortable: true,
      cell: (order) => <span className="text-sm text-charcoal">{order.supplierName}</span>,
    },
    {
      id: 'status',
      header: t('procurement.orders.status'),
      sortable: true,
      cell: (order) => (
        <StatusBadge label={t(STATUS_LABEL[order.status])} tone={STATUS_TONE[order.status]} />
      ),
    },
    {
      id: 'totalAmount',
      header: t('procurement.orders.total'),
      sortable: true,
      align: 'right',
      className: 'tabular-nums',
      cell: (order) => formatMoney(order.totalAmount),
    },
    {
      id: 'expectedDeliveryDate',
      header: t('procurement.orders.expectedDelivery'),
      sortable: true,
      cell: (order) => (
        <span className="whitespace-nowrap text-sm text-charcoal-light">
          {order.expectedDeliveryDate
            ? formatDate(order.expectedDeliveryDate)
            : t('procurement.orders.deliveryNotScheduled')}
        </span>
      ),
    },
    {
      id: 'createdAt',
      header: t('procurement.orders.createdAt'),
      sortable: true,
      cell: (order) => (
        <span className="whitespace-nowrap text-charcoal-light">
          {formatDateTime(order.createdAt)}
        </span>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <PageHeader
        title={t('procurement.orders.title')}
        description={t('procurement.orders.description')}
        actions={
          canCreate ? (
            <Button onClick={handleOpenCreate}>
              <Plus className="h-4 w-4" aria-hidden="true" />
              {t('procurement.create.action')}
            </Button>
          ) : null
        }
      />

      {lastCreated ? (
        <div
          role="status"
          aria-live="polite"
          className="flex items-start gap-2 rounded-lg border border-mint-accent bg-mint-accent/30 px-4 py-3 text-sm text-charcoal"
        >
          <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0" aria-hidden="true" />
          <p>{t('procurement.create.success', { orderNumber: lastCreated })}</p>
        </div>
      ) : null}

      <QuerySection
        query={ordersQuery}
        skeleton={<TableSkeleton columns={columns.length} rows={7} />}
        isEmpty={(data) => data.items.length === 0}
        emptyTitle={t('procurement.orders.empty.title')}
        emptyDescription={t('procurement.orders.empty.body')}
        emptyActionLabel={t('procurement.orders.empty.cta')}
      >
        {(data) => (
          <DataTable
            data={data.items}
            columns={columns}
            getRowId={(order) => order.id}
            caption={t('procurement.orders.tableCaption')}
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

      {isCreateOpen ? (
        <CreatePurchaseOrderDialog
          isOpen
          suppliers={suppliers}
          items={items}
          hasItemCatalogue={canReadItems}
          hasSupplierAccess={canReadSuppliers}
          isPending={createMutation.isPending}
          error={createMutation.error}
          onConfirm={handleCreate}
          onCancel={handleCloseCreate}
        />
      ) : null}
    </div>
  );
}
