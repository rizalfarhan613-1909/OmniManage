'use client';

import { useI18n } from '@/components/providers/LanguageProvider';

import { useMemo, useState } from 'react';

import { useSession } from '@/components/providers/SessionProvider';
import { DataTable, type DataTableColumn } from '@/components/shared/DataTable';
import { ForbiddenState } from '@/components/shared/ForbiddenState';
import { ModuleNotLicensedState } from '@/components/shared/ModuleNotLicensedState';
import { PageHeader } from '@/components/shared/PageHeader';
import { QuerySection } from '@/components/shared/QuerySection';
import { TableSkeleton } from '@/components/shared/Skeletons';
import { StatusBadge, type StatusTone } from '@/components/shared/StatusBadge';
import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import { useArInvoices } from '@/features/finance/api/use-ar-invoices';
import { useJournalEntries } from '@/features/finance/api/use-journal-entries';
import type { ArInvoice, ArInvoiceStatus, JournalEntry } from '@/features/finance/api/types';
import { useBusinessPartners } from '@/features/masterdata/api/use-business-partners';
import { MAX_PAGE_SIZE, toSortParam, type SortState } from '@/lib/api/pagination';

import { formatCount, formatDateTime, formatMoney } from '@/lib/utils/format';

const PAGE_SIZE = 10;

const INVOICE_TONE: Record<ArInvoiceStatus, StatusTone> = {
  Unpaid: 'warning',
  Paid: 'success',
  Cancelled: 'neutral',
};

const INVOICE_LABEL = {
  Unpaid: 'finance.status.unpaid',
  Paid: 'finance.status.paid',
  Cancelled: 'finance.status.cancelled',
} as const;

export function FinanceView() {
  const { t } = useI18n();
  const session = useSession();
  const [invoicePage, setInvoicePage] = useState(1);
  const [invoiceSort, setInvoiceSort] = useState<SortState>({
    field: 'createdAt',
    direction: 'desc',
  });
  const [journalPage, setJournalPage] = useState(1);
  const [journalSort, setJournalSort] = useState<SortState>({
    field: 'postedDateUtc',
    direction: 'desc',
  });

  const hasArModule = session.activeModules.includes(MODULE_CODES.finAr);
  const canReadInvoices = session.permissions.includes(PERMISSIONS.finance.invoiceRead);
  const canReadJournals = session.permissions.includes(PERMISSIONS.finance.journalRead);
  const canReadPartners = session.permissions.includes(PERMISSIONS.masterData.businessPartnerRead);

  const invoicesQuery = useArInvoices(
    { page: invoicePage, pageSize: PAGE_SIZE, sort: toSortParam(invoiceSort) },
    { enabled: hasArModule && canReadInvoices },
  );
  const journalsQuery = useJournalEntries(
    { page: journalPage, pageSize: PAGE_SIZE, sort: toSortParam(journalSort) },
    { enabled: canReadJournals },
  );
  const partnersQuery = useBusinessPartners(
    { page: 1, pageSize: MAX_PAGE_SIZE },
    { enabled: canReadPartners },
  );

  const partnerNames = useMemo(() => {
    const map = new Map<string, string>();

    for (const partner of partnersQuery.data?.items ?? []) {
      map.set(partner.id, partner.name);
    }

    return map;
  }, [partnersQuery.data]);

  const invoiceColumns: DataTableColumn<ArInvoice>[] = [
    {
      id: 'invoiceNumber',
      header: t('finance.ar.number'),
      sortable: true,
      cell: (invoice) => (
        <span className="font-mono text-xs font-semibold text-charcoal">
          {invoice.invoiceNumber}
        </span>
      ),
    },
    {
      id: 'customer',
      header: t('finance.ar.customer'),
      cell: (invoice) => (
        <span className="text-sm text-charcoal">
          {partnerNames.get(invoice.customerId) ?? t('finance.unknownCustomer')}
        </span>
      ),
    },
    {
      id: 'status',
      header: t('finance.ar.status'),
      sortable: true,
      cell: (invoice) => (
        <StatusBadge label={t(INVOICE_LABEL[invoice.status])} tone={INVOICE_TONE[invoice.status]} />
      ),
    },
    {
      id: 'totalAmount',
      header: t('finance.ar.total'),
      sortable: true,
      align: 'right',
      className: 'tabular-nums',
      cell: (invoice) => formatMoney(invoice.totalAmount),
    },
    {
      id: 'createdAt',
      header: t('finance.ar.createdAt'),
      sortable: true,
      cell: (invoice) => (
        <span className="whitespace-nowrap text-charcoal-light">
          {formatDateTime(invoice.createdAt)}
        </span>
      ),
    },
  ];

  const journalColumns: DataTableColumn<JournalEntry>[] = [
    {
      id: 'entryNumber',
      header: t('finance.journal.number'),
      sortable: true,
      cell: (entry) => (
        <span className="font-mono text-xs font-semibold text-charcoal">{entry.entryNumber}</span>
      ),
    },
    {
      id: 'postedDateUtc',
      header: t('finance.journal.postedAt'),
      sortable: true,
      cell: (entry) => (
        <span className="whitespace-nowrap text-charcoal-light">
          {formatDateTime(entry.postedDateUtc)}
        </span>
      ),
    },
    {
      id: 'description',
      header: t('finance.journal.descriptionColumn'),
      cell: (entry) => <span className="text-charcoal">{entry.description}</span>,
    },
    {
      id: 'debitTotal',
      header: t('finance.journal.debit'),
      align: 'right',
      className: 'tabular-nums',
      cell: (entry) => formatMoney(entry.debitTotal),
    },
    {
      id: 'creditTotal',
      header: t('finance.journal.credit'),
      align: 'right',
      className: 'tabular-nums',
      cell: (entry) => formatMoney(entry.creditTotal),
    },
    {
      id: 'lines',
      header: t('finance.journal.lines'),
      align: 'right',
      className: 'tabular-nums',
      cell: (entry) => formatCount(entry.lines.length),
    },
  ];

  return (
    <div className="space-y-8">
      <PageHeader title={t('nav.finance')} description={t('page.finance.description')} />

      <section aria-labelledby="ar-heading" className="space-y-4">
        <div>
          <h2 id="ar-heading" className="text-lg font-semibold text-charcoal">
            {t('finance.ar.title')}
          </h2>
          <p className="mt-1 text-sm text-charcoal-light">{t('finance.ar.description')}</p>
        </div>

        {!hasArModule ? (
          <ModuleNotLicensedState moduleCode={MODULE_CODES.finAr} />
        ) : !canReadInvoices ? (
          <ForbiddenState />
        ) : (
          <QuerySection
            query={invoicesQuery}
            skeleton={<TableSkeleton columns={invoiceColumns.length} rows={7} />}
            isEmpty={(data) => data.items.length === 0}
            emptyTitle={t('finance.ar.empty.title')}
            emptyDescription={t('finance.ar.empty.body')}
            emptyActionLabel={t('finance.ar.empty.cta')}
            moduleCode={MODULE_CODES.finAr}
          >
            {(data) => (
              <DataTable
                data={data.items}
                columns={invoiceColumns}
                getRowId={(invoice) => invoice.id}
                caption={t('finance.ar.tableCaption')}
                sort={invoiceSort}
                onSortChange={(next) => {
                  setInvoiceSort(next);
                  setInvoicePage(1);
                }}
                pagination={{
                  page: data.page,
                  pageSize: data.pageSize,
                  totalItems: data.totalItems,
                  totalPages: data.totalPages,
                }}
                onPageChange={setInvoicePage}
              />
            )}
          </QuerySection>
        )}
      </section>

      <section aria-labelledby="journal-heading" className="space-y-4">
        <div>
          <h2 id="journal-heading" className="text-lg font-semibold text-charcoal">
            {t('finance.journal.title')}
          </h2>
          <p className="mt-1 text-sm text-charcoal-light">{t('finance.journal.description')}</p>
        </div>

        {!canReadJournals ? (
          <ForbiddenState />
        ) : (
          <QuerySection
            query={journalsQuery}
            skeleton={<TableSkeleton columns={journalColumns.length} rows={7} />}
            isEmpty={(data) => data.items.length === 0}
            emptyTitle={t('finance.journal.empty.title')}
            emptyDescription={t('finance.journal.empty.body')}
            emptyActionLabel={t('finance.journal.empty.cta')}
            moduleCode={MODULE_CODES.finGl}
          >
            {(data) => (
              <DataTable
                data={data.items}
                columns={journalColumns}
                getRowId={(entry) => entry.id}
                caption={t('finance.journal.tableCaption')}
                sort={journalSort}
                onSortChange={(next) => {
                  setJournalSort(next);
                  setJournalPage(1);
                }}
                pagination={{
                  page: data.page,
                  pageSize: data.pageSize,
                  totalItems: data.totalItems,
                  totalPages: data.totalPages,
                }}
                onPageChange={setJournalPage}
              />
            )}
          </QuerySection>
        )}
      </section>
    </div>
  );
}
