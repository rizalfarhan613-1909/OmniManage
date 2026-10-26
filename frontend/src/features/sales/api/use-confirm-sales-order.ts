import { useMutation, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';
import { QUERY_KEY_ROOTS } from '@/lib/api/query-keys';

import { salesKeys } from './keys';
import type { SalesOrder } from './types';

/**
 * UC-01 command: `POST /api/v1/sales/orders/{id}/confirm`.
 *
 * The backend transaction writes the aggregate + outbox event. On success we
 * refresh every sales list and the executive dashboard (whose aggregates are
 * derived from confirmed orders) instead of mutating caches optimistically —
 * confirmation is a financial state transition (Rules.md §10 / DESIGN.md P8).
 */
export function useConfirmSalesOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (orderId: string) =>
      apiClient
        .post<SalesOrder>(`/api/v1/sales/orders/${orderId}/confirm`)
        .then((response) => response.data),
    onSuccess: (order) => {
      queryClient.setQueryData(salesKeys.order(order.id), order);

      return Promise.all([
        queryClient.invalidateQueries({ queryKey: salesKeys.orders() }),
        queryClient.invalidateQueries({ queryKey: [QUERY_KEY_ROOTS.dashboard] }),
      ]);
    },
  });
}
