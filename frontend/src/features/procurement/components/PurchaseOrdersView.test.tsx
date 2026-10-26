import { fireEvent, render, screen, within } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import type { Item } from '@/features/masterdata/api/types';
import type {
  CreatePurchaseOrderRequest,
  PurchaseOrder,
  Supplier,
} from '@/features/procurement/api/types';
import { createSession, pageOf } from '@/test/fixtures';
import { mutationResult, queryResult } from '@/test/query';

import { PurchaseOrdersView } from './PurchaseOrdersView';

const api = vi.hoisted(() => ({
  useSession: vi.fn(),
  useGetPurchaseOrders: vi.fn(),
  useGetSuppliers: vi.fn(),
  useCreatePurchaseOrder: vi.fn(),
  useItems: vi.fn(),
}));

vi.mock('@/components/providers/SessionProvider', () => ({
  useSession: api.useSession,
}));

vi.mock('@/features/procurement/api/useGetPurchaseOrders', () => ({
  useGetPurchaseOrders: api.useGetPurchaseOrders,
}));

vi.mock('@/features/procurement/api/useGetSuppliers', () => ({
  useGetSuppliers: api.useGetSuppliers,
}));

vi.mock('@/features/procurement/api/useCreatePurchaseOrder', () => ({
  useCreatePurchaseOrder: api.useCreatePurchaseOrder,
}));

vi.mock('@/features/masterdata/api/use-items', () => ({
  useItems: api.useItems,
}));

const DRAFT: PurchaseOrder = {
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

const APPROVED: PurchaseOrder = {
  ...DRAFT,
  id: 'po-2',
  orderNumber: 'PO-002',
  status: 'Approved',
  expectedDeliveryDate: null,
};

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

const ITEM: Item = {
  id: 'item-1',
  sku: 'SKU-001',
  name: 'Steel plate',
  uomCode: 'EA',
  category: 'Raw material',
  basePrice: '150000.0000',
  costPrice: '120000.0000',
  isStockTracked: true,
  isActive: true,
  createdAt: '2026-10-20T10:00:00Z',
};

beforeEach(() => {
  api.useSession.mockReturnValue(
    createSession({
      activeModules: [MODULE_CODES.procCore, MODULE_CODES.coreMasterData],
      permissions: [
        PERMISSIONS.procurement.purchaseOrderRead,
        PERMISSIONS.procurement.purchaseOrderCreate,
        PERMISSIONS.procurement.supplierRead,
        PERMISSIONS.masterData.itemRead,
      ],
    }),
  );
  api.useGetPurchaseOrders.mockReturnValue(queryResult({ data: pageOf([DRAFT, APPROVED]) }));
  api.useGetSuppliers.mockReturnValue(queryResult({ data: pageOf([SUPPLIER]) }));
  api.useItems.mockReturnValue(queryResult({ data: pageOf([ITEM]) }));
  api.useCreatePurchaseOrder.mockReturnValue(mutationResult());
});

describe('PurchaseOrdersView', () => {
  it('renders purchase orders with supplier snapshots, statuses and formatted totals', () => {
    render(<PurchaseOrdersView />);

    expect(screen.getByRole('table', { name: 'Purchase orders' })).toBeInTheDocument();
    expect(screen.getByText('PO-001')).toBeInTheDocument();
    expect(screen.getByText('PO-002')).toBeInTheDocument();
    expect(screen.getAllByText('Northwind Supply')).toHaveLength(2);
    expect(screen.getByText('Draft')).toBeInTheDocument();
    expect(screen.getByText('Approved')).toBeInTheDocument();
    expect(screen.getAllByText('IDR 1,500,000.00')).toHaveLength(2);
    expect(screen.getByText('Not scheduled')).toBeInTheDocument();
  });

  it('hides the create action without the purchase-order create permission', () => {
    api.useSession.mockReturnValue(
      createSession({
        activeModules: [MODULE_CODES.procCore],
        permissions: [PERMISSIONS.procurement.purchaseOrderRead],
      }),
    );

    render(<PurchaseOrdersView />);

    expect(screen.queryByRole('button', { name: 'New purchase order' })).not.toBeInTheDocument();
  });

  it('creates a purchase order with a supplier and one item line', () => {
    const mutate = vi.fn(
      (
        request: CreatePurchaseOrderRequest,
        options?: { onSuccess?: (order: PurchaseOrder) => void },
      ) => {
        expect(request).toEqual({
          orderNumber: 'PO-2026-0001',
          supplierId: 'sup-1',
          expectedDeliveryDate: null,
          notes: null,
          lines: [{ itemId: 'item-1', quantity: 10, unitPrice: 150000 }],
        });
        options?.onSuccess?.({ ...DRAFT, orderNumber: 'PO-2026-0001' });
      },
    );
    api.useCreatePurchaseOrder.mockReturnValue(mutationResult({ mutate }));

    render(<PurchaseOrdersView />);

    fireEvent.click(screen.getByRole('button', { name: 'New purchase order' }));

    const dialog = screen.getByRole('dialog');
    expect(
      within(dialog).getByRole('heading', { name: 'Create purchase order' }),
    ).toBeInTheDocument();

    fireEvent.change(within(dialog).getByLabelText('Order number'), {
      target: { value: 'PO-2026-0001' },
    });
    fireEvent.change(within(dialog).getByLabelText('Supplier'), {
      target: { value: 'sup-1' },
    });
    fireEvent.change(within(dialog).getByLabelText('Item'), {
      target: { value: 'item-1' },
    });
    fireEvent.change(within(dialog).getByLabelText('Quantity'), {
      target: { value: '10' },
    });
    fireEvent.change(within(dialog).getByLabelText('Unit price'), {
      target: { value: '150000' },
    });

    fireEvent.click(within(dialog).getByRole('button', { name: 'Create purchase order' }));

    expect(mutate).toHaveBeenCalledTimes(1);
    expect(screen.getByText('Purchase order PO-2026-0001 created.')).toBeInTheDocument();
  });

  it('renders the empty state when no orders exist', () => {
    api.useGetPurchaseOrders.mockReturnValue(queryResult({ data: pageOf([]) }));

    render(<PurchaseOrdersView />);

    expect(screen.getByText('No purchase orders yet')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Refresh purchase orders' })).toBeInTheDocument();
  });
});
