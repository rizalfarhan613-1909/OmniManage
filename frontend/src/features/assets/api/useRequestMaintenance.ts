import { useMutation, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';

import type { RequestMaintenanceRequest } from '../types';
import { assetsKeys } from './keys';

export interface RequestMaintenanceInput {
  assetId: string;
  request: RequestMaintenanceRequest;
}

/**
 * `POST /api/v1/assets/{assetId}/maintenance-orders`.
 *
 * The maintenance order and its outbox event are committed together by the
 * backend; both the asset register and the asset's maintenance board are
 * invalidated afterwards.
 */
export function useRequestMaintenance() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ assetId, request }: RequestMaintenanceInput) =>
      apiClient
        .post<string>(`/api/v1/assets/${assetId}/maintenance-orders`, request)
        .then((response) => response.data),
    onSuccess: (_maintenanceOrderId, { assetId }) =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: assetsKeys.assets() }),
        queryClient.invalidateQueries({ queryKey: assetsKeys.asset(assetId) }),
        queryClient.invalidateQueries({ queryKey: assetsKeys.maintenanceOrders(assetId) }),
      ]),
  });
}
