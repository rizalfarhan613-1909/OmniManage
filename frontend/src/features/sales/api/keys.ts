import type { ListQueryParams } from '@/lib/api/pagination';
import { QUERY_KEY_ROOTS } from '@/lib/api/query-keys';

export interface SalesOrderQueryParams extends ListQueryParams {
  status?: string;
}

export const salesKeys = {
  all: [QUERY_KEY_ROOTS.sales] as const,
  orders: () => [...salesKeys.all, 'orders'] as const,
  orderList: (params: SalesOrderQueryParams) => [...salesKeys.orders(), 'list', params] as const,
  order: (id: string) => [...salesKeys.orders(), 'detail', id] as const,
};
