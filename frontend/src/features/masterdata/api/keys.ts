import { QUERY_KEY_ROOTS } from '@/lib/api/query-keys';
import type { ListQueryParams } from '@/lib/api/pagination';

export const masterDataKeys = {
  all: [QUERY_KEY_ROOTS.masterData] as const,
  businessPartners: () => [...masterDataKeys.all, 'business-partners'] as const,
  businessPartnerList: (params: ListQueryParams) =>
    [...masterDataKeys.businessPartners(), 'list', params] as const,
  items: () => [...masterDataKeys.all, 'items'] as const,
  itemList: (params: ListQueryParams) => [...masterDataKeys.items(), 'list', params] as const,
};
