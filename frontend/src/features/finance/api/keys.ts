import type { ListQueryParams } from '@/lib/api/pagination';
import { QUERY_KEY_ROOTS } from '@/lib/api/query-keys';

export interface ArInvoiceQueryParams extends ListQueryParams {
  status?: string;
}

export interface JournalEntryQueryParams extends ListQueryParams {
  postedFromUtc?: string;
  postedToUtc?: string;
}

export const financeKeys = {
  all: [QUERY_KEY_ROOTS.finance] as const,
  invoices: () => [...financeKeys.all, 'ar-invoices'] as const,
  invoiceList: (params: ArInvoiceQueryParams) =>
    [...financeKeys.invoices(), 'list', params] as const,
  journals: () => [...financeKeys.all, 'journal-entries'] as const,
  journalList: (params: JournalEntryQueryParams) =>
    [...financeKeys.journals(), 'list', params] as const,
};
