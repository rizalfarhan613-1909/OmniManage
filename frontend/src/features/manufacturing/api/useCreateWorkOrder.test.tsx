import { renderHook } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { createFetchMock, errorResponse, successResponse } from '@/test/api';
import { createQueryWrapper, createTestQueryClient } from '@/test/query';

import { manufacturingKeys } from './keys';
import type { CreateWorkOrderRequest, WorkOrder } from './types';
import { useCreateWorkOrder } from './useCreateWorkOrder';

const fetchMock = createFetchMock();

const REQUEST: CreateWorkOrderRequest = {
  orderNumber: 'WO-2026-0001',
  billOfMaterialId: 'bom-1',
  quantityPlanned: 25,
  dueDate: '2026-11-05',
  notes: 'Run on line 2.',
};

const CREATED: WorkOrder = {
  id: 'wo-1',
  orderNumber: 'WO-2026-0001',
  productItemId: 'item-1',
  billOfMaterialId: 'bom-1',
  billOfMaterialCode: 'BOM-001',
  status: 'Draft',
  quantityPlanned: '25.0000',
  quantityProduced: '0.0000',
  dueDate: '2026-11-05',
  notes: 'Run on line 2.',
  createdAt: '2026-10-20T10:00:00Z',
  updatedAt: null,
};

beforeEach(() => {
  vi.stubGlobal('fetch', fetchMock);
});

afterEach(() => {
  vi.unstubAllGlobals();
});

describe('useCreateWorkOrder', () => {
  it('posts the command, seeds the detail cache and invalidates the work-order list', async () => {
    fetchMock.mockResolvedValue(successResponse(CREATED, 201));
    const queryClient = createTestQueryClient();
    const setQueryData = vi.spyOn(queryClient, 'setQueryData');
    const invalidateQueries = vi.spyOn(queryClient, 'invalidateQueries');

    const { result } = renderHook(() => useCreateWorkOrder(), {
      wrapper: createQueryWrapper(queryClient),
    });

    await result.current.mutateAsync(REQUEST);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/proxy/manufacturing/work-orders',
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify(REQUEST),
      }),
    );
    expect(setQueryData).toHaveBeenCalledWith(manufacturingKeys.workOrder('wo-1'), CREATED);
    expect(invalidateQueries).toHaveBeenCalledWith({
      queryKey: manufacturingKeys.workOrders(),
    });
  });

  it('surfaces the typed API failure without writing caches', async () => {
    fetchMock.mockResolvedValue(
      errorResponse('MANUFACTURING.WORK_ORDER_NUMBER_EXISTS', 'That number is taken.', 409),
    );
    const queryClient = createTestQueryClient();
    const setQueryData = vi.spyOn(queryClient, 'setQueryData');

    const { result } = renderHook(() => useCreateWorkOrder(), {
      wrapper: createQueryWrapper(queryClient),
    });

    await expect(result.current.mutateAsync(REQUEST)).rejects.toMatchObject({
      code: 'MANUFACTURING.WORK_ORDER_NUMBER_EXISTS',
      status: 409,
    });
    expect(setQueryData).not.toHaveBeenCalled();
  });
});
