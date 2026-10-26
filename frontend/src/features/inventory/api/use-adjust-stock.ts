import { useMutation, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';

import { inventoryKeys } from './keys';
import type { AdjustStockRequest, StockEntry } from './types';

/**
 * `POST /api/v1/inventory/stock/adjustments`.
 *
 * Appends an immutable stock ledger entry. Balances are derived projections, so
 * every stock balance query is invalidated rather than patched locally.
 */
export function useAdjustStock() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: AdjustStockRequest) =>
      apiClient
        .post<StockEntry>('/api/v1/inventory/stock/adjustments', request)
        .then((response) => response.data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: inventoryKeys.stock() }),
  });
}
