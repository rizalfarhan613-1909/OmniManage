'use client';

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
import { useGetBillOfMaterials } from '@/features/manufacturing/api/useGetBillOfMaterials';
import { useCreateWorkOrder } from '@/features/manufacturing/api/useCreateWorkOrder';
import { useGetWorkOrders } from '@/features/manufacturing/api/useGetWorkOrders';
import type {
  CreateWorkOrderRequest,
  WorkOrder,
  WorkOrderStatus,
} from '@/features/manufacturing/api/types';
import { useItems } from '@/features/masterdata/api/use-items';
import { MAX_PAGE_SIZE, toSortParam, type SortState } from '@/lib/api/pagination';
import { t } from '@/lib/i18n';
import { formatDate, formatDateTime, formatQuantity } from '@/lib/utils/format';

import { CreateWorkOrderDialog } from './CreateWorkOrderDialog';

const PAGE_SIZE = 10;

const STATUS_TONE: Record<WorkOrderStatus, StatusTone> = {
  Draft: 'warning',
  Released: 'info',
  InProgress: 'info',
  Completed: 'success',
  Cancelled: 'neutral',
};

const STATUS_LABEL = {
  Draft: 'manufacturing.workOrders.status.draft',
  Released: 'manufacturing.workOrders.status.released',
  InProgress: 'manufacturing.workOrders.status.inProgress',
  Completed: 'manufacturing.workOrders.status.completed',
  Cancelled: 'manufacturing.workOrders.status.cancelled',
} as const;

export function WorkOrdersView() {
  const session = useSession();
  const [page, setPage] = useState(1);
  const [sort, setSort] = useState<SortState>({ field: 'createdAt', direction: 'desc' });
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [lastCreated, setLastCreated] = useState<string | null>(null);

  const canCreate = session.permissions.includes(PERMISSIONS.manufacturing.workOrderCreate);
  const canReadBillOfMaterials = session.permissions.includes(
    PERMISSIONS.manufacturing.billOfMaterialRead,
  );
  const canReadItems =
    session.activeModules.includes(MODULE_CODES.coreMasterData) &&
    session.permissions.includes(PERMISSIONS.masterData.itemRead);

  const workOrdersQuery = useGetWorkOrders({
    page,
    pageSize: PAGE_SIZE,
    sort: toSortParam(sort),
  });
  const billOfMaterialsQuery = useGetBillOfMaterials(
    { page: 1, pageSize: MAX_PAGE_SIZE, isActive: true, sort: 'code' },
    { enabled: canCreate && canReadBillOfMaterials },
  );
  const itemsQuery = useItems({ page: 1, pageSize: MAX_PAGE_SIZE }, { enabled: canReadItems });

  const createMutation = useCreateWorkOrder();

  const billOfMaterials = billOfMaterialsQuery.data?.items ?? [];
  const items = itemsQuery.data?.items ?? [];

  const productLabel = (productItemId: string) => {
    const item = items.find((candidate) => candidate.id === productItemId);

    return item ? `${item.sku} \u2014 ${item.name}` : productItemId;
  };

  const handleOpenCreate = () => {
    createMutation.reset();
    setIsCreateOpen(true);
  };

  const handleCloseCreate = () => {
    if (!createMutation.isPending) {
      setIsCreateOpen(false);
    }
  };

  const handleCreate = (request: CreateWorkOrderRequest) => {
    createMutation.mutate(request, {
      onSuccess: (workOrder) => {
        setLastCreated(workOrder.orderNumber);
        setIsCreateOpen(false);
      },
    });
  };

  const columns: DataTableColumn<WorkOrder>[] = [
    {
      id: 'orderNumber',
      header: t('manufacturing.workOrders.number'),
      sortable: true,
      cell: (workOrder) => (
        <span className="font-mono text-xs font-semibold text-charcoal">
          {workOrder.orderNumber}
        </span>
      ),
    },
    {
      id: 'productItemId',
      header: t('manufacturing.workOrders.product'),
      sortable: true,
      cell: (workOrder) => (
        <span className="text-sm text-charcoal">{productLabel(workOrder.productItemId)}</span>
      ),
    },
    {
      id: 'billOfMaterialCode',
      header: t('manufacturing.workOrders.billOfMaterial'),
      cell: (workOrder) => (
        <span className="font-mono text-xs text-charcoal-light">
          {workOrder.billOfMaterialCode}
        </span>
      ),
    },
    {
      id: 'status',
      header: t('manufacturing.workOrders.status'),
      sortable: true,
      cell: (workOrder) => (
        <StatusBadge
          label={t(STATUS_LABEL[workOrder.status])}
          tone={STATUS_TONE[workOrder.status]}
        />
      ),
    },
    {
      id: 'quantityPlanned',
      header: t('manufacturing.workOrders.quantityPlanned'),
      sortable: true,
      align: 'right',
      className: 'tabular-nums',
      cell: (workOrder) => formatQuantity(workOrder.quantityPlanned),
    },
    {
      id: 'quantityProduced',
      header: t('manufacturing.workOrders.quantityProduced'),
      align: 'right',
      className: 'tabular-nums',
      cell: (workOrder) => formatQuantity(workOrder.quantityProduced),
    },
    {
      id: 'dueDate',
      header: t('manufacturing.workOrders.dueDate'),
      sortable: true,
      cell: (workOrder) => (
        <span className="whitespace-nowrap text-sm text-charcoal-light">
          {workOrder.dueDate
            ? formatDate(workOrder.dueDate)
            : t('manufacturing.workOrders.dueDateNotScheduled')}
        </span>
      ),
    },
    {
      id: 'createdAt',
      header: t('manufacturing.workOrders.createdAt'),
      sortable: true,
      cell: (workOrder) => (
        <span className="whitespace-nowrap text-charcoal-light">
          {formatDateTime(workOrder.createdAt)}
        </span>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <PageHeader
        title={t('manufacturing.workOrders.title')}
        description={t('manufacturing.workOrders.description')}
        actions={
          canCreate ? (
            <Button onClick={handleOpenCreate}>
              <Plus className="h-4 w-4" aria-hidden="true" />
              {t('manufacturing.create.action')}
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
          <p>{t('manufacturing.create.success', { orderNumber: lastCreated })}</p>
        </div>
      ) : null}

      <QuerySection
        query={workOrdersQuery}
        skeleton={<TableSkeleton columns={columns.length} rows={7} />}
        isEmpty={(data) => data.items.length === 0}
        emptyTitle={t('manufacturing.workOrders.empty.title')}
        emptyDescription={t('manufacturing.workOrders.empty.body')}
        emptyActionLabel={t('manufacturing.workOrders.empty.cta')}
      >
        {(data) => (
          <DataTable
            data={data.items}
            columns={columns}
            getRowId={(workOrder) => workOrder.id}
            caption={t('manufacturing.workOrders.tableCaption')}
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
        <CreateWorkOrderDialog
          isOpen
          billOfMaterials={billOfMaterials}
          items={items}
          hasBillOfMaterialAccess={canReadBillOfMaterials}
          isPending={createMutation.isPending}
          error={createMutation.error}
          onConfirm={handleCreate}
          onCancel={handleCloseCreate}
        />
      ) : null}
    </div>
  );
}
