import { fireEvent, render, screen, within } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import type { Item } from '@/features/masterdata/api/types';
import type {
  BillOfMaterial,
  CreateWorkOrderRequest,
  WorkOrder,
} from '@/features/manufacturing/api/types';
import { createSession, pageOf } from '@/test/fixtures';
import { mutationResult, queryResult } from '@/test/query';

import { WorkOrdersView } from './WorkOrdersView';

const api = vi.hoisted(() => ({
  useSession: vi.fn(),
  useGetWorkOrders: vi.fn(),
  useGetBillOfMaterials: vi.fn(),
  useCreateWorkOrder: vi.fn(),
  useItems: vi.fn(),
}));

vi.mock('@/components/providers/SessionProvider', () => ({
  useSession: api.useSession,
}));

vi.mock('@/features/manufacturing/api/useGetWorkOrders', () => ({
  useGetWorkOrders: api.useGetWorkOrders,
}));

vi.mock('@/features/manufacturing/api/useGetBillOfMaterials', () => ({
  useGetBillOfMaterials: api.useGetBillOfMaterials,
}));

vi.mock('@/features/manufacturing/api/useCreateWorkOrder', () => ({
  useCreateWorkOrder: api.useCreateWorkOrder,
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

const DRAFT: WorkOrder = {
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

const COMPLETED: WorkOrder = {
  ...DRAFT,
  id: 'wo-2',
  orderNumber: 'WO-002',
  status: 'Completed',
  quantityPlanned: '10.0000',
  quantityProduced: '10.0000',
  dueDate: null,
};

const BILL_OF_MATERIAL: BillOfMaterial = {
  id: 'bom-1',
  code: 'BOM-001',
  name: 'Steel frame',
  productItemId: 'item-1',
  version: 3,
  isActive: true,
  notes: null,
  components: [{ id: 'component-1', itemId: 'item-2', quantityPerUnit: '4.0000' }],
  createdAt: '2026-10-20T10:00:00Z',
  updatedAt: null,
};

beforeEach(() => {
  api.useSession.mockReturnValue(
    createSession({
      activeModules: [MODULE_CODES.mfgCore, MODULE_CODES.coreMasterData],
      permissions: [
        PERMISSIONS.manufacturing.workOrderRead,
        PERMISSIONS.manufacturing.workOrderCreate,
        PERMISSIONS.manufacturing.billOfMaterialRead,
        PERMISSIONS.masterData.itemRead,
      ],
    }),
  );
  api.useGetWorkOrders.mockReturnValue(queryResult({ data: pageOf([DRAFT, COMPLETED]) }));
  api.useGetBillOfMaterials.mockReturnValue(queryResult({ data: pageOf([BILL_OF_MATERIAL]) }));
  api.useItems.mockReturnValue(queryResult({ data: pageOf([ITEM]) }));
  api.useCreateWorkOrder.mockReturnValue(mutationResult());
});

describe('WorkOrdersView', () => {
  it('renders work orders with resolved products, statuses and formatted quantities', () => {
    render(<WorkOrdersView />);

    expect(screen.getByRole('table', { name: 'Manufacturing work orders' })).toBeInTheDocument();
    expect(screen.getByText('WO-001')).toBeInTheDocument();
    expect(screen.getByText('WO-002')).toBeInTheDocument();
    expect(screen.getAllByText('FG-001 — Steel frame')).toHaveLength(2);
    expect(screen.getAllByText('BOM-001')).toHaveLength(2);
    expect(screen.getByText('Draft')).toBeInTheDocument();
    expect(screen.getByText('Completed')).toBeInTheDocument();
    expect(screen.getByText('25')).toBeInTheDocument();
    expect(screen.getByText('Not scheduled')).toBeInTheDocument();
  });

  it('hides the create action without the work-order create permission', () => {
    api.useSession.mockReturnValue(
      createSession({
        activeModules: [MODULE_CODES.mfgCore],
        permissions: [PERMISSIONS.manufacturing.workOrderRead],
      }),
    );

    render(<WorkOrdersView />);

    expect(screen.queryByRole('button', { name: 'New work order' })).not.toBeInTheDocument();
  });

  it('creates a work order from an active bill of materials', () => {
    const mutate = vi.fn(
      (
        request: CreateWorkOrderRequest,
        options?: { onSuccess?: (workOrder: WorkOrder) => void },
      ) => {
        expect(request).toEqual({
          orderNumber: 'WO-2026-0001',
          billOfMaterialId: 'bom-1',
          quantityPlanned: 25,
          dueDate: null,
          notes: null,
        });
        options?.onSuccess?.({ ...DRAFT, orderNumber: 'WO-2026-0001' });
      },
    );
    api.useCreateWorkOrder.mockReturnValue(mutationResult({ mutate }));

    render(<WorkOrdersView />);

    fireEvent.click(screen.getByRole('button', { name: 'New work order' }));

    const dialog = screen.getByRole('dialog');
    expect(within(dialog).getByRole('heading', { name: 'Create work order' })).toBeInTheDocument();

    fireEvent.change(within(dialog).getByLabelText('Work order number'), {
      target: { value: 'WO-2026-0001' },
    });
    fireEvent.change(within(dialog).getByLabelText('Bill of materials'), {
      target: { value: 'bom-1' },
    });
    expect(within(dialog).getByText('FG-001 — Steel frame')).toBeInTheDocument();
    fireEvent.change(within(dialog).getByLabelText('Planned quantity'), {
      target: { value: '25' },
    });

    fireEvent.click(within(dialog).getByRole('button', { name: 'Create work order' }));

    expect(mutate).toHaveBeenCalledTimes(1);
    expect(screen.getByText('Work order WO-2026-0001 created.')).toBeInTheDocument();
  });

  it('renders the empty state when no work orders exist', () => {
    api.useGetWorkOrders.mockReturnValue(queryResult({ data: pageOf([]) }));

    render(<WorkOrdersView />);

    expect(screen.getByText('No work orders yet')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Refresh work orders' })).toBeInTheDocument();
  });
});
