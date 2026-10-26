import { useMutation, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';

import { manufacturingKeys } from './keys';
import type { CreateWorkOrderRequest, WorkOrder } from './types';

/**
 * `POST /api/v1/manufacturing/work-orders`.
 *
 * The backend validates the BOM (active), order-number uniqueness and the
 * planned quantity inside one transaction. The work-order list is refreshed
 * only after the server confirms the write; no optimistic insert is attempted.
 */
export function useCreateWorkOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateWorkOrderRequest) =>
      apiClient
        .post<WorkOrder>('/api/v1/manufacturing/work-orders', request)
        .then((response) => response.data),
    onSuccess: (workOrder) => {
      queryClient.setQueryData(manufacturingKeys.workOrder(workOrder.id), workOrder);

      return queryClient.invalidateQueries({ queryKey: manufacturingKeys.workOrders() });
    },
  });
}
