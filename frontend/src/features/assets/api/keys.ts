import type { ListQueryParams } from '@/lib/api/pagination';
import { QUERY_KEY_ROOTS } from '@/lib/api/query-keys';

import type { AssetStatus, MaintenanceStatus } from '../types';

export interface AssetQueryParams extends ListQueryParams {
  category?: string;
  status?: AssetStatus;
}

export interface MaintenanceOrderQueryParams extends ListQueryParams {
  assetId?: string;
  status?: MaintenanceStatus;
}

export const assetsKeys = {
  all: [QUERY_KEY_ROOTS.assets] as const,
  assets: () => [...assetsKeys.all, 'assets'] as const,
  assetList: (params: AssetQueryParams) => [...assetsKeys.assets(), 'list', params] as const,
  asset: (id: string) => [...assetsKeys.assets(), 'detail', id] as const,
  maintenanceOrders: (assetId?: string) =>
    [...assetsKeys.all, 'maintenance-orders', assetId ?? 'all'] as const,
  maintenanceOrderList: (params: MaintenanceOrderQueryParams) =>
    [...assetsKeys.maintenanceOrders(params.assetId), 'list', params] as const,
  maintenanceOrder: (id: string) =>
    [...assetsKeys.all, 'maintenance-orders', 'detail', id] as const,
};
