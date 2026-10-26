import type { ListQueryParams } from '@/lib/api/pagination';
import { QUERY_KEY_ROOTS } from '@/lib/api/query-keys';

export interface SupplierQueryParams extends ListQueryParams {
  isActive?: boolean;
}

export interface PurchaseOrderQueryParams extends ListQueryParams {
  status?: string;
  supplierId?: string;
}

export const procurementKeys = {
  all: [QUERY_KEY_ROOTS.procurement] as const,
  suppliers: () => [...procurementKeys.all, 'suppliers'] as const,
  supplierList: (params: SupplierQueryParams) =>
    [...procurementKeys.suppliers(), 'list', params] as const,
  purchaseOrders: () => [...procurementKeys.all, 'purchase-orders'] as const,
  purchaseOrderList: (params: PurchaseOrderQueryParams) =>
    [...procurementKeys.purchaseOrders(), 'list', params] as const,
  purchaseOrder: (id: string) => [...procurementKeys.purchaseOrders(), 'detail', id] as const,
};
