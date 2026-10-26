import { renderHook, waitFor } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import type { PagedResult } from '@/lib/api/pagination';
import { createFetchMock, successResponse } from '@/test/api';
import { createQueryWrapper, createTestQueryClient } from '@/test/query';

import { salesKeys } from './keys';
import type { SalesOrder } from './types';
import { useSalesOrders } from './use-sales-orders';

const fetchMock = createFetchMock();

const ORDER: SalesOrder = {
  id: 'so-1',
  orderNumber: 'SO-001',
  customerId: 'bp-1',
  status: 'Draft',
  totalAmount: '1500000',
  lines: [],
  createdAt: '2026-10-20T10:00:00Z',
  updatedAt: null,
};

const PAGE: PagedResult<SalesOrder> = {
  items: [ORDER],
  page: 2,
  pageSize: 10,
  totalItems: 11,
  totalPages: 2,
};

beforeEach(() => {
  vi.stubGlobal('fetch', fetchMock);
});

afterEach(() => {
  vi.unstubAllGlobals();
});

describe('useSalesOrders', () => {
  it('requests the documented endpoint with server-side paging and sorting', async () => {
    fetchMock.mockResolvedValue(successResponse(PAGE));
    const queryClient = createTestQueryClient();
    const params = { page: 2, pageSize: 10, sort: '-createdAt' };

    const { result } = renderHook(() => useSalesOrders(params), {
      wrapper: createQueryWrapper(queryClient),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(PAGE);
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/proxy/sales/orders?page=2&pageSize=10&sort=-createdAt',
      expect.objectContaining({ method: 'GET' }),
    );
    expect(queryClient.getQueryData(salesKeys.orderList(params))).toEqual(PAGE);
  });

  it('stays dormant when the module or permission gate is closed', async () => {
    const queryClient = createTestQueryClient();

    const { result } = renderHook(() => useSalesOrders({}, { enabled: false }), {
      wrapper: createQueryWrapper(queryClient),
    });

    // Give TanStack Query a chance to schedule a fetch before asserting.
    await waitFor(() => expect(result.current.fetchStatus).toBe('idle'));

    expect(fetchMock).not.toHaveBeenCalled();
  });
});
