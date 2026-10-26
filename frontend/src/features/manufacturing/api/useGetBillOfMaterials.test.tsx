import { renderHook, waitFor } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import type { PagedResult } from '@/lib/api/pagination';
import { createFetchMock, successResponse } from '@/test/api';
import { createQueryWrapper, createTestQueryClient } from '@/test/query';

import { manufacturingKeys } from './keys';
import type { BillOfMaterial } from './types';
import { useGetBillOfMaterials } from './useGetBillOfMaterials';

const fetchMock = createFetchMock();

const BILL_OF_MATERIAL: BillOfMaterial = {
  id: 'bom-1',
  code: 'BOM-001',
  name: 'Steel frame',
  productItemId: 'item-1',
  version: 2,
  isActive: true,
  notes: null,
  components: [
    { id: 'component-1', itemId: 'item-2', quantityPerUnit: '4.0000' },
    { id: 'component-2', itemId: 'item-3', quantityPerUnit: '1.5000' },
  ],
  createdAt: '2026-10-20T10:00:00Z',
  updatedAt: null,
};

const PAGE: PagedResult<BillOfMaterial> = {
  items: [BILL_OF_MATERIAL],
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

describe('useGetBillOfMaterials', () => {
  it('requests the documented endpoint with paging, sorting and filters', async () => {
    fetchMock.mockResolvedValue(successResponse(PAGE));
    const queryClient = createTestQueryClient();
    const params = {
      page: 2,
      pageSize: 10,
      sort: 'code',
      productItemId: 'item-1',
      isActive: true,
    };

    const { result } = renderHook(() => useGetBillOfMaterials(params), {
      wrapper: createQueryWrapper(queryClient),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(PAGE);
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/proxy/manufacturing/bom?page=2&pageSize=10&sort=code&productItemId=item-1&isActive=true',
      expect.objectContaining({ method: 'GET' }),
    );
    expect(queryClient.getQueryData(manufacturingKeys.billOfMaterialList(params))).toEqual(PAGE);
  });

  it('stays dormant when the module or permission gate is closed', async () => {
    const queryClient = createTestQueryClient();

    const { result } = renderHook(() => useGetBillOfMaterials({}, { enabled: false }), {
      wrapper: createQueryWrapper(queryClient),
    });

    await waitFor(() => expect(result.current.fetchStatus).toBe('idle'));

    expect(fetchMock).not.toHaveBeenCalled();
  });
});
