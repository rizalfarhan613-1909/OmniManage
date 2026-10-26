import { render, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import type { BillOfMaterial } from '@/features/manufacturing/api/types';
import type { Item } from '@/features/masterdata/api/types';
import { createSession, pageOf } from '@/test/fixtures';
import { queryResult } from '@/test/query';

import { BillOfMaterialsView } from './BillOfMaterialsView';

const api = vi.hoisted(() => ({
  useSession: vi.fn(),
  useGetBillOfMaterials: vi.fn(),
  useItems: vi.fn(),
}));

vi.mock('@/components/providers/SessionProvider', () => ({
  useSession: api.useSession,
}));

vi.mock('@/features/manufacturing/api/useGetBillOfMaterials', () => ({
  useGetBillOfMaterials: api.useGetBillOfMaterials,
}));

vi.mock('@/features/masterdata/api/use-items', () => ({
  useItems: api.useItems,
}));

const ITEM: Item = {
  id: 'item-1',
  sku: 'FG-001',
  name: 'Steel frame',
  uomCode: 'EA',
  category: 'Finished goods',
  basePrice: '250000.0000',
  costPrice: '180000.0000',
  isStockTracked: true,
  isActive: true,
  createdAt: '2026-10-20T10:00:00Z',
};

const ACTIVE: BillOfMaterial = {
  id: 'bom-1',
  code: 'BOM-001',
  name: 'Steel frame',
  productItemId: 'item-1',
  version: 3,
  isActive: true,
  notes: null,
  components: [
    { id: 'component-1', itemId: 'item-2', quantityPerUnit: '4.0000' },
    { id: 'component-2', itemId: 'item-3', quantityPerUnit: '1.5000' },
  ],
  createdAt: '2026-10-20T10:00:00Z',
  updatedAt: null,
};

const INACTIVE: BillOfMaterial = {
  ...ACTIVE,
  id: 'bom-2',
  code: 'BOM-002',
  name: 'Steel frame v2',
  version: 1,
  isActive: false,
  components: [],
};

beforeEach(() => {
  api.useSession.mockReturnValue(
    createSession({
      activeModules: [MODULE_CODES.mfgCore, MODULE_CODES.coreMasterData],
      permissions: [PERMISSIONS.manufacturing.billOfMaterialRead, PERMISSIONS.masterData.itemRead],
    }),
  );
  api.useGetBillOfMaterials.mockReturnValue(queryResult({ data: pageOf([ACTIVE, INACTIVE]) }));
  api.useItems.mockReturnValue(queryResult({ data: pageOf([ITEM]) }));
});

describe('BillOfMaterialsView', () => {
  it('renders BOM versions, components, products and active state', () => {
    render(<BillOfMaterialsView />);

    expect(screen.getByRole('table', { name: 'Bills of materials' })).toBeInTheDocument();
    expect(screen.getByText('BOM-001')).toBeInTheDocument();
    expect(screen.getByText('BOM-002')).toBeInTheDocument();
    expect(screen.getAllByText('FG-001 — Steel frame')).toHaveLength(2);
    expect(screen.getByText('v3')).toBeInTheDocument();
    expect(screen.getByText('v1')).toBeInTheDocument();
    expect(screen.getByText('2 components')).toBeInTheDocument();
    expect(screen.getByText('0 components')).toBeInTheDocument();
    expect(screen.getByText('Active')).toBeInTheDocument();
    expect(screen.getByText('Inactive')).toBeInTheDocument();
  });

  it('renders the empty state when no bills of materials exist', () => {
    api.useGetBillOfMaterials.mockReturnValue(queryResult({ data: pageOf([]) }));

    render(<BillOfMaterialsView />);

    expect(screen.getByText('No bills of materials yet')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Refresh bills of materials' })).toBeInTheDocument();
  });
});
