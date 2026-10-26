import { renderHook, waitFor } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import type { PagedResult } from '@/lib/api/pagination';
import { createFetchMock, successResponse } from '@/test/api';
import { createQueryWrapper, createTestQueryClient } from '@/test/query';

import { procurementKeys } from './keys';
import type { PurchaseOrder } from './types';
import { useGetPurchaseOrders } from './useGetPurchaseOrders';

const fetchMock = createFetchMock();

const ORDER: PurchaseOrder = {
  id: 'po-1',
  orderNumber: 'PO-001',
  supplierId: 'sup-1',
  supplierName: 'Northwind Supply',
  status: 'Draft',
  totalAmount: '1500000.0000',
  expectedDeliveryDate: '2026-11-05',
  notes: null,
  lines: [],
  createdAt: '2026-10-20T10:00:00Z',
  updatedAt: null,
};

const PAGE: PagedResult<PurchaseOrder> = {
  items: [ORDER],
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

describe('useGetPurchaseOrders', () => {
  it('requests the documented endpoint with paging, sorting and filters', async () => {
    fetchMock.mockResolvedValue(successResponse(PAGE));
    const queryClient = createTestQueryClient();
    const params = {
      page: 1,
      pageSize: 10,
      sort: '-createdAt',
      status: 'Draft',
      supplierId: 'sup-1',
    };

    const { result } = renderHook(() => useGetPurchaseOrders(params), {
      wrapper: createQueryWrapper(queryClient),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(PAGE);
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/proxy/procurement/purchase-orders?page=1&pageSize=10&sort=-createdAt&status=Draft&supplierId=sup-1',
      expect.objectContaining({ method: 'GET' }),
    );
    expect(queryClient.getQueryData(procurementKeys.purchaseOrderList(params))).toEqual(PAGE);
  });

  it('stays dormant when the module or permission gate is closed', async () => {
    const queryClient = createTestQueryClient();

    const { result } = renderHook(() => useGetPurchaseOrders({}, { enabled: false }), {
      wrapper: createQueryWrapper(queryClient),
    });

    await waitFor(() => expect(result.current.fetchStatus).toBe('idle'));

    expect(fetchMock).not.toHaveBeenCalled();
  });
});
