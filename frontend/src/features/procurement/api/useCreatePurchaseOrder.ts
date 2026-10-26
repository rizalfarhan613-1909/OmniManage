import { useMutation, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';

import { procurementKeys } from './keys';
import type { CreatePurchaseOrderRequest, PurchaseOrder } from './types';

/**
 * `POST /api/v1/procurement/purchase-orders`.
 *
 * The backend validates the supplier, order number uniqueness and every line
 * inside one transaction. The purchase-order list is refreshed only after the
 * server confirms the write; no optimistic insert is attempted.
 */
export function useCreatePurchaseOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreatePurchaseOrderRequest) =>
      apiClient
        .post<PurchaseOrder>('/api/v1/procurement/purchase-orders', request)
        .then((response) => response.data),
    onSuccess: (order) => {
      queryClient.setQueryData(procurementKeys.purchaseOrder(order.id), order);

      return queryClient.invalidateQueries({ queryKey: procurementKeys.purchaseOrders() });
    },
  });
}
