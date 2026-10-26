import { renderHook, waitFor } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import type { PagedResult } from '@/lib/api/pagination';
import { createFetchMock, successResponse } from '@/test/api';
import { createQueryWrapper, createTestQueryClient } from '@/test/query';

import { manufacturingKeys } from './keys';
import type { WorkOrder } from './types';
import { useGetWorkOrders } from './useGetWorkOrders';

const fetchMock = createFetchMock();

const WORK_ORDER: WorkOrder = {
  id: 'wo-1',
  orderNumber: 'WO-001',
  productItemId: 'item-1',
  billOfMaterialId: 'bom-1',
  billOfMaterialCode: 'BOM-001',
  status: 'Draft',
  quantityPlanned: '25.0000',
  quantityProduced: '0.0000',
  dueDate: '2026-11-05',
  notes: null,
  createdAt: '2026-10-20T10:00:00Z',
  updatedAt: null,
};

const PAGE: PagedResult<WorkOrder> = {
  items: [WORK_ORDER],
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

describe('useGetWorkOrders', () => {
  it('requests the documented endpoint with paging, sorting and filters', async () => {
    fetchMock.mockResolvedValue(successResponse(PAGE));
    const queryClient = createTestQueryClient();
    const params = {
      page: 1,
      pageSize: 10,
      sort: '-createdAt',
      status: 'Draft' as const,
      productItemId: 'item-1',
      billOfMaterialId: 'bom-1',
    };

    const { result } = renderHook(() => useGetWorkOrders(params), {
      wrapper: createQueryWrapper(queryClient),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(PAGE);
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/proxy/manufacturing/work-orders?page=1&pageSize=10&sort=-createdAt&status=Draft&productItemId=item-1&billOfMaterialId=bom-1',
      expect.objectContaining({ method: 'GET' }),
    );
    expect(queryClient.getQueryData(manufacturingKeys.workOrderList(params))).toEqual(PAGE);
  });

  it('stays dormant when the module or permission gate is closed', async () => {
    const queryClient = createTestQueryClient();

    const { result } = renderHook(() => useGetWorkOrders({}, { enabled: false }), {
      wrapper: createQueryWrapper(queryClient),
    });

    await waitFor(() => expect(result.current.fetchStatus).toBe('idle'));

    expect(fetchMock).not.toHaveBeenCalled();
  });
});
