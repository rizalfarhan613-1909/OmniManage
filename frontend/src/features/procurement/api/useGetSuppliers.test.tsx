import { renderHook, waitFor } from '@testing-library/react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import type { PagedResult } from '@/lib/api/pagination';
import { createFetchMock, successResponse } from '@/test/api';
import { createQueryWrapper, createTestQueryClient } from '@/test/query';

import { procurementKeys } from './keys';
import type { Supplier } from './types';
import { useGetSuppliers } from './useGetSuppliers';

const fetchMock = createFetchMock();

const SUPPLIER: Supplier = {
  id: 'sup-1',
  code: 'SUP-001',
  name: 'Northwind Supply',
  contactName: 'Dana Reed',
  email: 'dana@northwind.test',
  phone: '+62 21 555 0100',
  paymentTermsDays: 30,
  isActive: true,
  createdAt: '2026-10-20T10:00:00Z',
  updatedAt: null,
};

const PAGE: PagedResult<Supplier> = {
  items: [SUPPLIER],
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

describe('useGetSuppliers', () => {
  it('requests the documented endpoint with paging, sorting and the active filter', async () => {
    fetchMock.mockResolvedValue(successResponse(PAGE));
    const queryClient = createTestQueryClient();
    const params = { page: 2, pageSize: 10, sort: 'name', isActive: true };

    const { result } = renderHook(() => useGetSuppliers(params), {
      wrapper: createQueryWrapper(queryClient),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data).toEqual(PAGE);
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/proxy/procurement/suppliers?page=2&pageSize=10&sort=name&isActive=true',
      expect.objectContaining({ method: 'GET' }),
    );
    expect(queryClient.getQueryData(procurementKeys.supplierList(params))).toEqual(PAGE);
  });

  it('stays dormant when the module or permission gate is closed', async () => {
    const queryClient = createTestQueryClient();

    const { result } = renderHook(() => useGetSuppliers({}, { enabled: false }), {
      wrapper: createQueryWrapper(queryClient),
    });

    await waitFor(() => expect(result.current.fetchStatus).toBe('idle'));

    expect(fetchMock).not.toHaveBeenCalled();
  });
});
