'use client';

import { Plus, Wrench, Trash2 } from 'lucide-react';
import { useState } from 'react';

import { useSession } from '@/components/providers/SessionProvider';
import { DataTable, type DataTableColumn } from '@/components/shared/DataTable';
import { PageHeader } from '@/components/shared/PageHeader';
import { QuerySection } from '@/components/shared/QuerySection';
import { TableSkeleton } from '@/components/shared/Skeletons';
import { StatusBadge, type StatusTone } from '@/components/shared/StatusBadge';
import { Button } from '@/components/ui/Button';
import { PERMISSIONS } from '@/config/permissions';
import { useGetAssets } from '@/features/assets/api/useGetAssets';
import { useCreateAsset } from '@/features/assets/api/useCreateAsset';
import { useRequestMaintenance } from '@/features/assets/api/useRequestMaintenance';
import type {
  AssetDto,
  AssetStatus,
  RequestMaintenanceRequest,
  CreateAssetRequest,
} from '@/features/assets/types';
import { toSortParam, type SortState } from '@/lib/api/pagination';
import { t, type TranslationKey } from '@/lib/i18n';
import { formatMoney } from '@/lib/utils/format';

import { CreateAssetDialog } from './CreateAssetDialog';
import { MaintenanceDialog } from './MaintenanceDialog';

const PAGE_SIZE = 10;

const STATUS_TONE: Record<AssetStatus, StatusTone> = {
  Active: 'success',
  UnderMaintenance: 'info',
  Retired: 'neutral',
};

const STATUS_LABEL = {
  Active: 'assets.register.status.active',
  UnderMaintenance: 'assets.register.status.underMaintenance',
  Retired: 'assets.register.status.retired',
} as const;
export function AssetRegisterView() {
  const session = useSession();
  const [page, setPage] = useState(1);
  const [sort, setSort] = useState<SortState>({ field: 'createdAt', direction: 'desc' });
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [isMaintenanceOpen, setIsMaintenanceOpen] = useState<{
    assetId: string;
    assetName: string;
  } | null>(null);
  const [lastCreated, setLastCreated] = useState<TranslationKey | null>(null);
  const [lastMaintenance, setLastMaintenance] = useState<TranslationKey | null>(null);

  const canCreateAsset = session.permissions.includes(PERMISSIONS.assets.assetCreate);
  const canRequestMaintenance = session.permissions.includes(PERMISSIONS.assets.maintenanceRequest);

  const assetsQuery = useGetAssets({
    page,
    pageSize: PAGE_SIZE,
    sort: toSortParam(sort),
  });

  const createAssetMutation = useCreateAsset();
  const maintenanceMutation = useRequestMaintenance();

  const handleOpenCreate = () => {
    createAssetMutation.reset();
    setIsCreateOpen(true);
  };

  const handleCloseCreate = () => {
    if (!createAssetMutation.isPending) {
      setIsCreateOpen(false);
    }
  };

  const handleCreateAsset = (request: CreateAssetRequest) => {
    createAssetMutation.mutate(request, {
      onSuccess: () => {
        setLastCreated('assets.register.create.success');
        setIsCreateOpen(false);
      },
    });
  };

  const handleOpenMaintenance = (assetId: string, assetName: string) => {
    setIsMaintenanceOpen({ assetId, assetName });
  };

  const handleCloseMaintenance = () => {
    setIsMaintenanceOpen(null);
  };

  const handleRequestMaintenance = (request: RequestMaintenanceRequest) => {
    maintenanceMutation.mutate(
      { assetId: isMaintenanceOpen?.assetId ?? '', request },
      {
        onSuccess: () => {
          setLastMaintenance('assets.register.maintenance.success');
          handleCloseMaintenance();
        },
      },
    );
  };
  const columns: DataTableColumn<AssetDto>[] = [
    {
      id: 'assetCode',
      header: t('assets.register.assetCode'),
      sortable: true,
      cell: (asset) => (
        <span className="font-mono text-xs font-semibold text-charcoal">{asset.assetCode}</span>
      ),
    },
    {
      id: 'name',
      header: t('assets.register.name'),
      sortable: true,
      cell: (asset) => <span className="text-charcoal">{asset.name}</span>,
    },
    {
      id: 'category',
      header: t('assets.register.category'),
      sortable: true,
      cell: (asset) => <span className="text-charcoal-light">{asset.category}</span>,
    },
    {
      id: 'serialNumber',
      header: t('assets.register.serialNumber'),
      sortable: true,
      cell: (asset) => (
        <span className="font-mono text-xs text-charcoal-light">{asset.serialNumber ?? '—'}</span>
      ),
    },
    {
      id: 'status',
      header: t('assets.register.status'),
      sortable: true,
      cell: (asset) => (
        <StatusBadge label={t(STATUS_LABEL[asset.status])} tone={STATUS_TONE[asset.status]} />
      ),
    },
    {
      id: 'purchaseValue',
      header: t('assets.register.purchaseValue'),
      sortable: true,
      align: 'right',
      cell: (asset) => (
        <span className="font-mono text-charcoal">{formatMoney(asset.purchaseValue)}</span>
      ),
    },
    {
      id: 'acquisitionDate',
      header: t('assets.register.acquisitionDate'),
      sortable: true,
      cell: (asset) => (
        <span className="whitespace-nowrap text-sm text-charcoal-light">
          {asset.acquisitionDate}
        </span>
      ),
    },
    {
      id: 'actions',
      header: t('assets.register.actions'),
      align: 'right',
      cell: (asset) => (
        <div className="flex items-center justify-end gap-1">
          {canRequestMaintenance && asset.status !== 'Retired' && (
            <Button
              variant="ghost"
              size="sm"
              onClick={() => handleOpenMaintenance(asset.id, asset.name)}
            >
              <Wrench className="h-3 w-3" aria-hidden="true" />
            </Button>
          )}
          {canCreateAsset && (
            <Button variant="ghost" size="sm" onClick={() => {}}>
              <Trash2 className="h-3 w-3 text-red-500" aria-hidden="true" />
            </Button>
          )}
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <PageHeader
        title={t('assets.register.title')}
        description={t('assets.register.description')}
        actions={
          canCreateAsset ? (
            <Button onClick={handleOpenCreate}>
              <Plus className="h-4 w-4" aria-hidden="true" />
              {t('assets.register.create.submit')}
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
          <Plus className="mt-0.5 h-4 w-4 shrink-0" aria-hidden="true" />
          <p>{t(lastCreated)}</p>
        </div>
      ) : null}

      {lastMaintenance ? (
        <div
          role="status"
          aria-live="polite"
          className="flex items-start gap-2 rounded-lg border border-mint-accent bg-mint-accent/30 px-4 py-3 text-sm text-charcoal"
        >
          <Wrench className="mt-0.5 h-4 w-4 shrink-0" aria-hidden="true" />
          <p>{t(lastMaintenance)}</p>
        </div>
      ) : null}

      <QuerySection
        query={assetsQuery}
        skeleton={<TableSkeleton columns={columns.length} rows={7} />}
        isEmpty={(data) => data.items.length === 0}
        emptyTitle={t('assets.register.empty.title')}
        emptyDescription={t('assets.register.empty.body')}
        emptyActionLabel={t('assets.register.empty.cta')}
      >
        {(data) => (
          <DataTable
            data={data.items}
            columns={columns}
            getRowId={(asset) => asset.id}
            caption={t('assets.register.tableCaption')}
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

      <CreateAssetDialog
        isOpen={isCreateOpen}
        isPending={createAssetMutation.isPending}
        error={createAssetMutation.error}
        onConfirm={handleCreateAsset}
        onCancel={handleCloseCreate}
      />

      {isMaintenanceOpen && (
        <MaintenanceDialog
          isOpen
          assetId={isMaintenanceOpen.assetId}
          assetName={isMaintenanceOpen.assetName}
          isPending={maintenanceMutation.isPending}
          error={maintenanceMutation.error}
          onConfirm={handleRequestMaintenance}
          onCancel={handleCloseMaintenance}
        />
      )}
    </div>
  );
}
