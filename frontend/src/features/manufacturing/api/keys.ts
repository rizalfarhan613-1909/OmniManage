import type { ListQueryParams } from '@/lib/api/pagination';
import { QUERY_KEY_ROOTS } from '@/lib/api/query-keys';

import type { WorkOrderStatus } from './types';

export interface WorkOrderQueryParams extends ListQueryParams {
  status?: WorkOrderStatus;
  productItemId?: string;
  billOfMaterialId?: string;
}

export interface BillOfMaterialQueryParams extends ListQueryParams {
  productItemId?: string;
  isActive?: boolean;
}

export const manufacturingKeys = {
  all: [QUERY_KEY_ROOTS.manufacturing] as const,
  workOrders: () => [...manufacturingKeys.all, 'work-orders'] as const,
  workOrderList: (params: WorkOrderQueryParams) =>
    [...manufacturingKeys.workOrders(), 'list', params] as const,
  workOrder: (id: string) => [...manufacturingKeys.workOrders(), 'detail', id] as const,
  billOfMaterials: () => [...manufacturingKeys.all, 'bom'] as const,
  billOfMaterialList: (params: BillOfMaterialQueryParams) =>
    [...manufacturingKeys.billOfMaterials(), 'list', params] as const,
};
