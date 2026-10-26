import { renderHook } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { QUERY_KEY_ROOTS } from '@/lib/api/query-keys';
import { createFetchMock, errorResponse, successResponse } from '@/test/api';
import { createQueryWrapper, createTestQueryClient } from '@/test/query';

import { salesKeys } from './keys';
import type { SalesOrder } from './types';
import { useConfirmSalesOrder } from './use-confirm-sales-order';

const fetchMock = createFetchMock();

const DRAFT: SalesOrder = {
  id: 'so-1',
  orderNumber: 'SO-001',
  customerId: 'bp-1',
  status: 'Draft',
  totalAmount: '1500000',
  lines: [],
  createdAt: '2026-10-20T10:00:00Z',
  updatedAt: null,
};

const CONFIRMED: SalesOrder = {
  ...DRAFT,
  status: 'Confirmed',
  updatedAt: '2026-10-22T09:00:00Z',
};

const LIST = {
  items: [DRAFT],
  page: 1,
  pageSize: 10,
  totalItems: 1,
  totalPages: 1,
};

beforeEach(() => {
  vi.stubGlobal('fetch', fetchMock);
});

afterEach(() => {
  vi.unstubAllGlobals();
});

describe('useConfirmSalesOrder', () => {
  it('posts the UC-01 command, seeds the detail cache and invalidates aggregates', async () => {
    fetchMock.mockResolvedValue(successResponse(CONFIRMED));
    const queryClient = createTestQueryClient();
    queryClient.setQueryData(salesKeys.orderList({}), LIST);

    const setQueryData = vi.spyOn(queryClient, 'setQueryData');
    const invalidateQueries = vi.spyOn(queryClient, 'invalidateQueries');

    const { result } = renderHook(() => useConfirmSalesOrder(), {
      wrapper: createQueryWrapper(queryClient),
    });

    await result.current.mutateAsync('so-1');

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/proxy/sales/orders/so-1/confirm',
      expect.objectContaining({ method: 'POST' }),
    );
    expect(setQueryData).toHaveBeenCalledWith(salesKeys.order('so-1'), CONFIRMED);
    expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: salesKeys.orders() });
    expect(invalidateQueries).toHaveBeenCalledWith({
      queryKey: [QUERY_KEY_ROOTS.dashboard],
    });
    expect(queryClient.getQueryData(salesKeys.orderList({}))).toEqual(LIST);
  });

  it('surfaces the typed API failure without writing caches', async () => {
    fetchMock.mockResolvedValue(
      errorResponse('CONCURRENCY_CONFLICT', 'Order was already confirmed.', 409),
    );
    const queryClient = createTestQueryClient();
    const setQueryData = vi.spyOn(queryClient, 'setQueryData');

    const { result } = renderHook(() => useConfirmSalesOrder(), {
      wrapper: createQueryWrapper(queryClient),
    });

    await expect(result.current.mutateAsync('so-1')).rejects.toMatchObject({
      code: 'CONCURRENCY_CONFLICT',
      status: 409,
    });
    expect(setQueryData).not.toHaveBeenCalled();
  });
});
