'use client';

import { useState } from 'react';

import { useSession } from '@/components/providers/SessionProvider';
import { DataTable, type DataTableColumn } from '@/components/shared/DataTable';
import { PageHeader } from '@/components/shared/PageHeader';
import { QuerySection } from '@/components/shared/QuerySection';
import { TableSkeleton } from '@/components/shared/Skeletons';
import { StatusBadge } from '@/components/shared/StatusBadge';
import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import { useGetBillOfMaterials } from '@/features/manufacturing/api/useGetBillOfMaterials';
import type { BillOfMaterial } from '@/features/manufacturing/api/types';
import { useItems } from '@/features/masterdata/api/use-items';
import { MAX_PAGE_SIZE, toSortParam, type SortState } from '@/lib/api/pagination';
import { t } from '@/lib/i18n';
import { formatDateTime } from '@/lib/utils/format';

const PAGE_SIZE = 10;

export function BillOfMaterialsView() {
  const session = useSession();
  const [page, setPage] = useState(1);
  const [sort, setSort] = useState<SortState>({ field: 'code', direction: 'asc' });

  const canReadItems =
    session.activeModules.includes(MODULE_CODES.coreMasterData) &&
    session.permissions.includes(PERMISSIONS.masterData.itemRead);

  const billOfMaterialsQuery = useGetBillOfMaterials({
    page,
    pageSize: PAGE_SIZE,
    sort: toSortParam(sort),
  });
  const itemsQuery = useItems({ page: 1, pageSize: MAX_PAGE_SIZE }, { enabled: canReadItems });

  const items = itemsQuery.data?.items ?? [];

  const productLabel = (productItemId: string) => {
    const item = items.find((candidate) => candidate.id === productItemId);

    return item ? `${item.sku} \u2014 ${item.name}` : productItemId;
  };

  const columns: DataTableColumn<BillOfMaterial>[] = [
    {
      id: 'code',
      header: t('manufacturing.bom.code'),
      sortable: true,
      cell: (bom) => (
        <span className="font-mono text-xs font-semibold text-charcoal">{bom.code}</span>
      ),
    },
    {
      id: 'name',
      header: t('manufacturing.bom.name'),
      sortable: true,
      cell: (bom) => <span className="text-sm text-charcoal">{bom.name}</span>,
    },
    {
      id: 'productItemId',
      header: t('manufacturing.bom.product'),
      cell: (bom) => (
        <span className="text-sm text-charcoal">{productLabel(bom.productItemId)}</span>
      ),
    },
    {
      id: 'version',
      header: t('manufacturing.bom.version'),
      sortable: true,
      align: 'right',
      className: 'tabular-nums',
      cell: (bom) => t('manufacturing.bom.versionValue', { version: bom.version }),
    },
    {
      id: 'components',
      header: t('manufacturing.bom.components'),
      align: 'right',
      className: 'tabular-nums',
      cell: (bom) => t('manufacturing.bom.componentCount', { count: bom.components.length }),
    },
    {
      id: 'isActive',
      header: t('manufacturing.bom.status'),
      sortable: true,
      cell: (bom) => (
        <StatusBadge
          label={
            bom.isActive
              ? t('manufacturing.bom.status.active')
              : t('manufacturing.bom.status.inactive')
          }
          tone={bom.isActive ? 'success' : 'neutral'}
        />
      ),
    },
    {
      id: 'createdAt',
      header: t('manufacturing.bom.createdAt'),
      sortable: true,
      cell: (bom) => (
        <span className="whitespace-nowrap text-charcoal-light">
          {formatDateTime(bom.createdAt)}
        </span>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <PageHeader
        title={t('manufacturing.bom.title')}
        description={t('manufacturing.bom.description')}
      />

      <QuerySection
        query={billOfMaterialsQuery}
        skeleton={<TableSkeleton columns={columns.length} rows={7} />}
        isEmpty={(data) => data.items.length === 0}
        emptyTitle={t('manufacturing.bom.empty.title')}
        emptyDescription={t('manufacturing.bom.empty.body')}
        emptyActionLabel={t('manufacturing.bom.empty.cta')}
      >
        {(data) => (
          <DataTable
            data={data.items}
            columns={columns}
            getRowId={(bom) => bom.id}
            caption={t('manufacturing.bom.tableCaption')}
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
    </div>
  );
}
