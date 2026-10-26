import { renderHook } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { createFetchMock, errorResponse, successResponse } from '@/test/api';
import { createQueryWrapper, createTestQueryClient } from '@/test/query';

import { procurementKeys } from './keys';
import type { CreatePurchaseOrderRequest, PurchaseOrder } from './types';
import { useCreatePurchaseOrder } from './useCreatePurchaseOrder';

const fetchMock = createFetchMock();

const REQUEST: CreatePurchaseOrderRequest = {
  orderNumber: 'PO-2026-0001',
  supplierId: 'sup-1',
  expectedDeliveryDate: '2026-11-05',
  notes: 'Deliver to the main warehouse.',
  lines: [{ itemId: 'item-1', quantity: 10, unitPrice: 150000 }],
};

const CREATED: PurchaseOrder = {
  id: 'po-1',
  orderNumber: 'PO-2026-0001',
  supplierId: 'sup-1',
  supplierName: 'Northwind Supply',
  status: 'Draft',
  totalAmount: '1500000.0000',
  expectedDeliveryDate: '2026-11-05',
  notes: 'Deliver to the main warehouse.',
  lines: [
    {
      id: 'line-1',
      itemId: 'item-1',
      quantity: '10.0000',
      unitPrice: '150000.0000',
      lineTotal: '1500000.0000',
    },
  ],
  createdAt: '2026-10-20T10:00:00Z',
  updatedAt: null,
};

beforeEach(() => {
  vi.stubGlobal('fetch', fetchMock);
});

afterEach(() => {
  vi.unstubAllGlobals();
});

describe('useCreatePurchaseOrder', () => {
  it('posts the command, seeds the detail cache and invalidates the order list', async () => {
    fetchMock.mockResolvedValue(successResponse(CREATED, 201));
    const queryClient = createTestQueryClient();
    const setQueryData = vi.spyOn(queryClient, 'setQueryData');
    const invalidateQueries = vi.spyOn(queryClient, 'invalidateQueries');

    const { result } = renderHook(() => useCreatePurchaseOrder(), {
      wrapper: createQueryWrapper(queryClient),
    });

    await result.current.mutateAsync(REQUEST);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/proxy/procurement/purchase-orders',
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify(REQUEST),
      }),
    );
    expect(setQueryData).toHaveBeenCalledWith(procurementKeys.purchaseOrder('po-1'), CREATED);
    expect(invalidateQueries).toHaveBeenCalledWith({
      queryKey: procurementKeys.purchaseOrders(),
    });
  });

  it('surfaces the typed API failure without writing caches', async () => {
    fetchMock.mockResolvedValue(
      errorResponse('VALIDATION_FAILED', 'The order number is invalid.', 400),
    );
    const queryClient = createTestQueryClient();
    const setQueryData = vi.spyOn(queryClient, 'setQueryData');

    const { result } = renderHook(() => useCreatePurchaseOrder(), {
      wrapper: createQueryWrapper(queryClient),
    });

    await expect(result.current.mutateAsync(REQUEST)).rejects.toMatchObject({
      code: 'VALIDATION_FAILED',
      status: 400,
    });
    expect(setQueryData).not.toHaveBeenCalled();
  });
});
