'use client';

import { useState } from 'react';

import { DataTable, type DataTableColumn } from '@/components/shared/DataTable';
import { PageHeader } from '@/components/shared/PageHeader';
import { QuerySection } from '@/components/shared/QuerySection';
import { TableSkeleton } from '@/components/shared/Skeletons';
import { StatusBadge } from '@/components/shared/StatusBadge';
import type { Supplier } from '@/features/procurement/api/types';
import { useGetSuppliers } from '@/features/procurement/api/useGetSuppliers';
import { toSortParam, type SortState } from '@/lib/api/pagination';
import { t } from '@/lib/i18n';

const PAGE_SIZE = 10;

export function SuppliersView() {
  const [page, setPage] = useState(1);
  const [sort, setSort] = useState<SortState>({ field: 'name', direction: 'asc' });

  const suppliersQuery = useGetSuppliers({
    page,
    pageSize: PAGE_SIZE,
    sort: toSortParam(sort),
  });

  const columns: DataTableColumn<Supplier>[] = [
    {
      id: 'code',
      header: t('procurement.suppliers.code'),
      sortable: true,
      cell: (supplier) => (
        <span className="font-mono text-xs font-semibold text-charcoal">{supplier.code}</span>
      ),
    },
    {
      id: 'name',
      header: t('procurement.suppliers.name'),
      sortable: true,
      cell: (supplier) => <span className="font-medium text-charcoal">{supplier.name}</span>,
    },
    {
      id: 'contactName',
      header: t('procurement.suppliers.contact'),
      cell: (supplier) => (
        <span className="text-sm text-charcoal">
          {supplier.contactName ?? t('procurement.suppliers.notRecorded')}
        </span>
      ),
    },
    {
      id: 'email',
      header: t('procurement.suppliers.email'),
      cell: (supplier) => (
        <span className="text-sm text-charcoal">
          {supplier.email ?? t('procurement.suppliers.notRecorded')}
        </span>
      ),
    },
    {
      id: 'phone',
      header: t('procurement.suppliers.phone'),
      cell: (supplier) => (
        <span className="whitespace-nowrap text-sm text-charcoal">
          {supplier.phone ?? t('procurement.suppliers.notRecorded')}
        </span>
      ),
    },
    {
      id: 'paymentTermsDays',
      header: t('procurement.suppliers.paymentTerms'),
      align: 'right',
      className: 'tabular-nums',
      cell: (supplier) =>
        t('procurement.suppliers.paymentTermsValue', { days: supplier.paymentTermsDays }),
    },
    {
      id: 'isActive',
      header: t('procurement.suppliers.status'),
      cell: (supplier) => (
        <StatusBadge
          label={
            supplier.isActive
              ? t('procurement.suppliers.status.active')
              : t('procurement.suppliers.status.inactive')
          }
          tone={supplier.isActive ? 'success' : 'neutral'}
        />
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <PageHeader
        title={t('procurement.suppliers.title')}
        description={t('procurement.suppliers.description')}
      />

      <QuerySection
        query={suppliersQuery}
        skeleton={<TableSkeleton columns={columns.length} rows={7} />}
        isEmpty={(data) => data.items.length === 0}
        emptyTitle={t('procurement.suppliers.empty.title')}
        emptyDescription={t('procurement.suppliers.empty.body')}
        emptyActionLabel={t('procurement.suppliers.empty.cta')}
      >
        {(data) => (
          <DataTable
            data={data.items}
            columns={columns}
            getRowId={(supplier) => supplier.id}
            caption={t('procurement.suppliers.tableCaption')}
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
