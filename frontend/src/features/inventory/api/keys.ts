import { QUERY_KEY_ROOTS } from '@/lib/api/query-keys';
import type { ListQueryParams } from '@/lib/api/pagination';

export interface StockBalanceQueryParams {
  page?: number;
  pageSize?: number;
  warehouseId?: string;
  itemId?: string;
}

export const inventoryKeys = {
  all: [QUERY_KEY_ROOTS.inventory] as const,
  warehouses: () => [...inventoryKeys.all, 'warehouses'] as const,
  warehouseList: (params: ListQueryParams) =>
    [...inventoryKeys.warehouses(), 'list', params] as const,
  stock: () => [...inventoryKeys.all, 'stock'] as const,
  stockBalances: (params: StockBalanceQueryParams) =>
    [...inventoryKeys.stock(), 'balances', params] as const,
};
