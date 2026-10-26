import { useMutation, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';

import { inventoryKeys } from './keys';
import type { CreateWarehouseRequest, Warehouse } from './types';

/**
 * `POST /api/v1/inventory/warehouses`.
 *
 * The server normalizes the warehouse code (trim + uppercase) and enforces
 * uniqueness per tenant. The list is invalidated once the write is confirmed;
 * no optimistic insert is attempted.
 */
export function useCreateWarehouse() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateWarehouseRequest) =>
      apiClient
        .post<Warehouse>('/api/v1/inventory/warehouses', request)
        .then((response) => response.data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: inventoryKeys.warehouses() }),
  });
}
