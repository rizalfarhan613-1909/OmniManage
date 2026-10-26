import { useMutation, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/lib/api/client';

import type { CreateAssetRequest } from '../types';
import { assetsKeys } from './keys';

/**
 * `POST /api/v1/assets`.
 *
 * Returns the new asset identifier. The asset register is refreshed after the
 * server confirms the write; no optimistic insert is attempted.
 */
export function useCreateAsset() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateAssetRequest) =>
      apiClient.post<string>('/api/v1/assets', request).then((response) => response.data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: assetsKeys.assets() }),
  });
}
