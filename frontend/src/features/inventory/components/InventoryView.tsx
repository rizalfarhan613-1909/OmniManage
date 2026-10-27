'use client';

import { useI18n } from '@/components/providers/LanguageProvider';

import { ArrowLeftRight, Boxes, MapPin, Plus } from 'lucide-react';
import { useMemo, useState } from 'react';

import { useSession } from '@/components/providers/SessionProvider';
import { DataTable, type DataTableColumn } from '@/components/shared/DataTable';
import { ForbiddenState } from '@/components/shared/ForbiddenState';
import { PageHeader } from '@/components/shared/PageHeader';
import { QuerySection } from '@/components/shared/QuerySection';
import { ListSkeleton, TableSkeleton } from '@/components/shared/Skeletons';
import { StatusBadge } from '@/components/shared/StatusBadge';
import { Button } from '@/components/ui/Button';
import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import { useAdjustStock } from '@/features/inventory/api/use-adjust-stock';
import { useCreateWarehouse } from '@/features/inventory/api/use-create-warehouse';
import { useStockBalances } from '@/features/inventory/api/use-stock-balances';
import { useWarehouses } from '@/features/inventory/api/use-warehouses';
import type {
  AdjustStockRequest,
  CreateWarehouseRequest,
  StockBalance,
  Warehouse,
} from '@/features/inventory/api/types';
import { useItems } from '@/features/masterdata/api/use-items';
import { MAX_PAGE_SIZE } from '@/lib/api/pagination';

import { formatDateTime, formatQuantity } from '@/lib/utils/format';

import { AdjustStockDialog } from './AdjustStockDialog';
import { CreateWarehouseDialog } from './CreateWarehouseDialog';

const PAGE_SIZE = 10;

type AdjustTarget = {
  warehouseId: string;
  itemId: string;
};

export function InventoryView() {
  const { t } = useI18n();
  const session = useSession();
  const [page, setPage] = useState(1);
  const [warehouseId, setWarehouseId] = useState('');
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [adjustTarget, setAdjustTarget] = useState<AdjustTarget | null>(null);
  const [warehouseCreated, setWarehouseCreated] = useState(false);
  const [stockAdjusted, setStockAdjusted] = useState(false);

  const canReadWarehouses = session.permissions.includes(PERMISSIONS.inventory.warehouseRead);
  const canCreateWarehouse = session.permissions.includes(PERMISSIONS.inventory.warehouseCreate);
  const canAdjustStock = session.permissions.includes(PERMISSIONS.inventory.stockAdjust);
  const canReadItems =
    session.activeModules.includes(MODULE_CODES.coreMasterData) &&
    session.permissions.includes(PERMISSIONS.masterData.itemRead);

  const warehousesQuery = useWarehouses(
    { page: 1, pageSize: MAX_PAGE_SIZE },
    { enabled: canReadWarehouses },
  );
  const itemsQuery = useItems({ page: 1, pageSize: MAX_PAGE_SIZE }, { enabled: canReadItems });
  const balancesQuery = useStockBalances({
    page,
    pageSize: PAGE_SIZE,
    warehouseId: warehouseId || undefined,
  });

  const createWarehouseMutation = useCreateWarehouse();
  const adjustStockMutation = useAdjustStock();

  const warehouses = warehousesQuery.data?.items ?? [];
  const items = itemsQuery.data?.items ?? [];
  const canOpenAdjust = canAdjustStock && canReadWarehouses && warehouses.length > 0;

  const itemNames = useMemo(() => {
    const map = new Map<string, { sku: string; name: string }>();

    for (const item of itemsQuery.data?.items ?? []) {
      map.set(item.id, { sku: item.sku, name: item.name });
    }

    return map;
  }, [itemsQuery.data]);

  const warehouseNames = useMemo(() => {
    const map = new Map<string, Warehouse>();

    for (const warehouse of warehousesQuery.data?.items ?? []) {
      map.set(warehouse.id, warehouse);
    }

    return map;
  }, [warehousesQuery.data]);

  const handleOpenCreate = () => {
    createWarehouseMutation.reset();
    setIsCreateOpen(true);
  };

  const handleCloseCreate = () => {
    if (!createWarehouseMutation.isPending) {
      setIsCreateOpen(false);
    }
  };

  const handleCreateWarehouse = (request: CreateWarehouseRequest) => {
    createWarehouseMutation.mutate(request, {
      onSuccess: () => {
        setWarehouseCreated(true);
        setIsCreateOpen(false);
      },
    });
  };

  const handleOpenAdjust = (target: AdjustTarget) => {
    adjustStockMutation.reset();
    setAdjustTarget(target);
  };

  const handleCloseAdjust = () => {
    if (!adjustStockMutation.isPending) {
      setAdjustTarget(null);
    }
  };

  const handleAdjustStock = (request: AdjustStockRequest) => {
    adjustStockMutation.mutate(request, {
      onSuccess: () => {
        setStockAdjusted(true);
        setAdjustTarget(null);
      },
    });
  };

  const columns: DataTableColumn<StockBalance>[] = [
    {
      id: 'item',
      header: t('inventory.stock.item'),
      cell: (balance) => {
        const item = itemNames.get(balance.itemId);

        return (
          <div className="min-w-0">
            <p className="truncate font-mono text-xs font-semibold text-charcoal">
              {item?.sku ?? t('inventory.stock.unknownItem')}
            </p>
            <p className="truncate text-xs text-charcoal-light">{item?.name ?? balance.itemId}</p>
          </div>
        );
      },
    },
    {
      id: 'warehouse',
      header: t('inventory.stock.warehouse'),
      cell: (balance) => {
        const warehouse = warehouseNames.get(balance.warehouseId);

        return (
          <div className="min-w-0">
            <p className="truncate text-sm text-charcoal">
              {warehouse?.name ?? t('inventory.stock.unknownWarehouse')}
            </p>
            <p className="truncate font-mono text-xs text-charcoal-light">
              {warehouse?.code ?? balance.warehouseId}
            </p>
          </div>
        );
      },
    },
    {
      id: 'quantityOnHand',
      header: t('inventory.stock.quantity'),
      align: 'right',
      className: 'tabular-nums',
      cell: (balance) => formatQuantity(balance.quantityOnHand),
    },
  ];

  if (canOpenAdjust) {
    columns.push({
      id: 'actions',
      header: t('inventory.stock.actions'),
      align: 'right',
      cell: (balance) => {
        const item = itemNames.get(balance.itemId);

        return (
          <Button
            variant="secondary"
            size="sm"
            aria-label={`${t('inventory.stock.adjust.rowAction')} ${item?.sku ?? balance.itemId}`}
            onClick={() =>
              handleOpenAdjust({ warehouseId: balance.warehouseId, itemId: balance.itemId })
            }
          >
            <ArrowLeftRight className="h-3 w-3" aria-hidden="true" />
            {t('inventory.stock.adjust.rowAction')}
          </Button>
        );
      },
    });
  }

  return (
    <div className="space-y-8">
      <PageHeader title={t('nav.inventory')} description={t('page.inventory.description')} />

      {warehouseCreated ? (
        <div
          role="status"
          aria-live="polite"
          className="flex items-start gap-2 rounded-lg border border-mint-accent bg-mint-accent/30 px-4 py-3 text-sm text-charcoal"
        >
          <Plus className="mt-0.5 h-4 w-4 shrink-0" aria-hidden="true" />
          <p>{t('inventory.warehouses.create.success')}</p>
        </div>
      ) : null}

      {stockAdjusted ? (
        <div
          role="status"
          aria-live="polite"
          className="flex items-start gap-2 rounded-lg border border-mint-accent bg-mint-accent/30 px-4 py-3 text-sm text-charcoal"
        >
          <ArrowLeftRight className="mt-0.5 h-4 w-4 shrink-0" aria-hidden="true" />
          <p>{t('inventory.stock.adjust.success')}</p>
        </div>
      ) : null}

      <section aria-labelledby="warehouses-heading" className="space-y-4">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <h2 id="warehouses-heading" className="text-lg font-semibold text-charcoal">
              {t('inventory.warehouses.title')}
            </h2>
            <p className="mt-1 text-sm text-charcoal-light">
              {t('inventory.warehouses.description')}
            </p>
          </div>

          {canCreateWarehouse ? (
            <Button onClick={handleOpenCreate}>
              <Plus className="h-4 w-4" aria-hidden="true" />
              {t('inventory.warehouses.create.action')}
            </Button>
          ) : null}
        </div>

        {canReadWarehouses ? (
          <QuerySection
            query={warehousesQuery}
            skeleton={<ListSkeleton rows={3} />}
            isEmpty={(data) => data.items.length === 0}
            emptyTitle={t('inventory.warehouses.empty.title')}
            emptyDescription={t('inventory.warehouses.empty.body')}
            emptyActionLabel={t('inventory.warehouses.empty.cta')}
          >
            {(data) => (
              <ul className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
                {data.items.map((warehouse) => (
                  <li
                    key={warehouse.id}
                    className="flex flex-col rounded-lg border border-line bg-surface p-4 shadow-sm transition-all duration-300 hover:-translate-y-1 hover:shadow-md"
                  >
                    <div className="flex items-start justify-between gap-3">
                      <div className="min-w-0">
                        <p className="font-mono text-xs font-semibold text-charcoal">
                          {warehouse.code}
                        </p>
                        <h3 className="mt-1 truncate text-sm font-semibold text-charcoal">
                          {warehouse.name}
                        </h3>
                      </div>
                      <StatusBadge
                        label={
                          warehouse.isActive
                            ? t('inventory.status.active')
                            : t('inventory.status.inactive')
                        }
                        tone={warehouse.isActive ? 'success' : 'neutral'}
                      />
                    </div>
                    <p className="mt-3 flex items-start gap-1.5 text-xs text-charcoal-light">
                      <MapPin className="mt-0.5 h-3.5 w-3.5 shrink-0" aria-hidden="true" />
                      {warehouse.address ?? t('inventory.warehouses.addressMissing')}
                    </p>
                    <p className="mt-auto pt-3 text-xs text-charcoal-light">
                      {t('inventory.warehouses.createdAt', {
                        date: formatDateTime(warehouse.createdAt),
                      })}
                    </p>
                  </li>
                ))}
              </ul>
            )}
          </QuerySection>
        ) : (
          <ForbiddenState />
        )}
      </section>

      <section aria-labelledby="stock-heading" className="space-y-4">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <h2 id="stock-heading" className="text-lg font-semibold text-charcoal">
              {t('inventory.stock.title')}
            </h2>
            <p className="mt-1 text-sm text-charcoal-light">{t('inventory.stock.description')}</p>
          </div>

          <div className="flex w-full flex-col gap-2 sm:w-auto sm:flex-row sm:items-end">
            {canOpenAdjust ? (
              <Button onClick={() => handleOpenAdjust({ warehouseId, itemId: '' })}>
                <ArrowLeftRight className="h-4 w-4" aria-hidden="true" />
                {t('inventory.stock.adjust.action')}
              </Button>
            ) : null}

            {canReadWarehouses && warehouses.length > 0 ? (
              <div className="w-full sm:w-64">
                <label
                  htmlFor="warehouse-filter"
                  className="block text-xs font-semibold uppercase tracking-wide text-charcoal-light"
                >
                  {t('inventory.stock.filter.label')}
                </label>
                <select
                  id="warehouse-filter"
                  value={warehouseId}
                  onChange={(event) => {
                    setWarehouseId(event.target.value);
                    setPage(1);
                  }}
                  className="mt-1 h-10 w-full rounded-md border border-line bg-surface px-3 text-sm text-charcoal shadow-sm focus-visible:border-teal-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary"
                >
                  <option value="">{t('inventory.stock.filter.all')}</option>
                  {warehouses.map((warehouse) => (
                    <option key={warehouse.id} value={warehouse.id}>
                      {warehouse.code} — {warehouse.name}
                    </option>
                  ))}
                </select>
              </div>
            ) : null}
          </div>
        </div>

        <QuerySection
          query={balancesQuery}
          skeleton={<TableSkeleton columns={columns.length} rows={7} />}
          isEmpty={(data) => data.items.length === 0}
          emptyTitle={t('inventory.stock.empty.title')}
          emptyDescription={t('inventory.stock.empty.body')}
          emptyActionLabel={t('inventory.stock.empty.cta')}
        >
          {(data) => (
            <DataTable
              data={data.items}
              columns={columns}
              getRowId={(balance) => `${balance.warehouseId}-${balance.itemId}`}
              caption={t('inventory.stock.tableCaption')}
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
      </section>

      <div className="flex items-center gap-2 rounded-lg border border-line bg-surface px-4 py-3 text-xs text-charcoal-light">
        <Boxes className="h-4 w-4 text-teal-primary" aria-hidden="true" />
        <p>{t('inventory.stock.ledgerNote')}</p>
      </div>

      {isCreateOpen ? (
        <CreateWarehouseDialog
          isOpen
          isPending={createWarehouseMutation.isPending}
          error={createWarehouseMutation.error}
          onConfirm={handleCreateWarehouse}
          onCancel={handleCloseCreate}
        />
      ) : null}

      {adjustTarget ? (
        <AdjustStockDialog
          isOpen
          warehouses={warehouses}
          items={items}
          initialWarehouseId={adjustTarget.warehouseId}
          initialItemId={adjustTarget.itemId}
          isPending={adjustStockMutation.isPending}
          error={adjustStockMutation.error}
          onConfirm={handleAdjustStock}
          onCancel={handleCloseAdjust}
        />
      ) : null}
    </div>
  );
}
