import { fireEvent, render, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { MODULE_CODES } from '@/config/module-codes';
import { PERMISSIONS } from '@/config/permissions';
import type { StockBalance, Warehouse } from '@/features/inventory/api/types';
import type { Item } from '@/features/masterdata/api/types';
import { API_ERROR_CODES, ApiRequestError } from '@/lib/api/errors';
import { createSession, pageOf } from '@/test/fixtures';
import { mutationResult, queryResult } from '@/test/query';

import { InventoryView } from './InventoryView';

const api = vi.hoisted(() => ({
  useSession: vi.fn(),
  useWarehouses: vi.fn(),
  useItems: vi.fn(),
  useStockBalances: vi.fn(),
  useCreateWarehouse: vi.fn(),
  useAdjustStock: vi.fn(),
}));

vi.mock('@/components/providers/SessionProvider', () => ({
  useSession: api.useSession,
}));

vi.mock('@/features/inventory/api/use-warehouses', () => ({
  useWarehouses: api.useWarehouses,
}));

vi.mock('@/features/masterdata/api/use-items', () => ({
  useItems: api.useItems,
}));

vi.mock('@/features/inventory/api/use-stock-balances', () => ({
  useStockBalances: api.useStockBalances,
}));

vi.mock('@/features/inventory/api/use-create-warehouse', () => ({
  useCreateWarehouse: api.useCreateWarehouse,
}));

vi.mock('@/features/inventory/api/use-adjust-stock', () => ({
  useAdjustStock: api.useAdjustStock,
}));

const WAREHOUSE: Warehouse = {
  id: 'wh-1',
  code: 'WH-1',
  name: 'Main Warehouse',
  address: '1 Market Street',
  isActive: true,
  createdAt: '2026-01-10T08:00:00Z',
};

const ITEM: Item = {
  id: 'item-1',
  sku: 'SKU-001',
  name: 'Industrial Widget',
  uomCode: 'PCS',
  category: 'Components',
  basePrice: '250000',
  costPrice: '150000',
  isStockTracked: true,
  isActive: true,
  createdAt: '2026-01-01T00:00:00Z',
};

const BALANCE: StockBalance = {
  itemId: 'item-1',
  warehouseId: 'wh-1',
  quantityOnHand: '12.5',
};

const READ_SESSION = createSession({
  activeModules: [MODULE_CODES.invCore, MODULE_CODES.coreMasterData],
  permissions: [
    PERMISSIONS.inventory.warehouseRead,
    PERMISSIONS.inventory.stockRead,
    PERMISSIONS.masterData.itemRead,
  ],
});

beforeEach(() => {
  api.useSession.mockReturnValue(READ_SESSION);
  api.useWarehouses.mockReturnValue(queryResult({ data: pageOf([WAREHOUSE]) }));
  api.useItems.mockReturnValue(queryResult({ data: pageOf([ITEM]) }));
  api.useStockBalances.mockReturnValue(queryResult({ data: pageOf([BALANCE]) }));
  api.useCreateWarehouse.mockReturnValue(mutationResult());
  api.useAdjustStock.mockReturnValue(mutationResult());
});

describe('InventoryView', () => {
  it('renders warehouse cards and the stock balance table', () => {
    render(<InventoryView />);

    expect(screen.getAllByText('WH-1')).toHaveLength(2);
    expect(screen.getAllByText('Main Warehouse')).toHaveLength(2);
    expect(screen.getByText('1 Market Street')).toBeInTheDocument();
    expect(screen.getByText('SKU-001')).toBeInTheDocument();
    expect(screen.getByText('Industrial Widget')).toBeInTheDocument();
    expect(screen.getByText('12.5')).toBeInTheDocument();
  });

  it('shows forbidden for warehouses when the read permission is missing', () => {
    api.useSession.mockReturnValue(
      createSession({
        activeModules: [MODULE_CODES.invCore, MODULE_CODES.coreMasterData],
        permissions: [PERMISSIONS.inventory.stockRead, PERMISSIONS.masterData.itemRead],
      }),
    );

    render(<InventoryView />);

    expect(screen.getByText("You don't have access")).toBeInTheDocument();
    expect(screen.getByText('SKU-001')).toBeInTheDocument();
  });

  it('renders the loading skeleton while balances load', () => {
    api.useStockBalances.mockReturnValue(queryResult({ isLoading: true }));

    render(<InventoryView />);

    expect(screen.getByRole('status')).toHaveTextContent('Loading content');
  });

  it('renders the empty balance state', () => {
    api.useStockBalances.mockReturnValue(queryResult({ data: pageOf([]) }));

    render(<InventoryView />);

    expect(screen.getByText('No stock balances yet')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Refresh balances' })).toBeInTheDocument();
  });

  it('renders the error state with the traceId', () => {
    api.useStockBalances.mockReturnValue(
      queryResult({
        error: new ApiRequestError({
          code: API_ERROR_CODES.unexpected,
          message: 'Balances are unavailable.',
          status: 500,
          traceId: 'trace-inventory-500',
        }),
      }),
    );

    render(<InventoryView />);

    expect(screen.getByText('Something went wrong')).toBeInTheDocument();
    expect(screen.getByText('trace-inventory-500')).toBeInTheDocument();
  });

  it('renders module-not-licensed when the stock API is not licensed', () => {
    api.useStockBalances.mockReturnValue(
      queryResult({
        error: new ApiRequestError({
          code: API_ERROR_CODES.moduleNotLicensed,
          message: 'Module not licensed.',
          status: 403,
          traceId: 'trace-inventory-licensed',
        }),
      }),
    );

    render(<InventoryView />);

    expect(screen.getByText('Module not active for your company')).toBeInTheDocument();
  });

  it('hides write actions when the user lacks create/adjust permissions', () => {
    render(<InventoryView />);

    expect(screen.queryByRole('button', { name: 'New warehouse' })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Adjust stock' })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Adjust SKU-001' })).not.toBeInTheDocument();
  });

  it('creates a warehouse from the dialog', () => {
    const mutate = vi.fn();
    api.useSession.mockReturnValue(
      createSession({
        activeModules: [MODULE_CODES.invCore, MODULE_CODES.coreMasterData],
        permissions: [...READ_SESSION.permissions, PERMISSIONS.inventory.warehouseCreate],
      }),
    );
    api.useCreateWarehouse.mockReturnValue(mutationResult({ mutate }));

    render(<InventoryView />);

    fireEvent.click(screen.getByRole('button', { name: 'New warehouse' }));
    fireEvent.change(screen.getByLabelText('Warehouse code'), {
      target: { value: ' wh-2 ' },
    });
    fireEvent.change(screen.getByLabelText('Warehouse name'), {
      target: { value: 'Overflow Warehouse' },
    });
    fireEvent.change(screen.getByLabelText('Address (optional)'), {
      target: { value: ' 2 Harbor Road ' },
    });
    fireEvent.click(screen.getByRole('button', { name: 'Create warehouse' }));

    expect(mutate).toHaveBeenCalledWith(
      { code: 'wh-2', name: 'Overflow Warehouse', address: '2 Harbor Road' },
      expect.anything(),
    );
  });

  it('posts a stock adjustment from a balance row', () => {
    const mutate = vi.fn();
    api.useSession.mockReturnValue(
      createSession({
        activeModules: [MODULE_CODES.invCore, MODULE_CODES.coreMasterData],
        permissions: [...READ_SESSION.permissions, PERMISSIONS.inventory.stockAdjust],
      }),
    );
    api.useAdjustStock.mockReturnValue(mutationResult({ mutate }));

    render(<InventoryView />);

    fireEvent.click(screen.getByRole('button', { name: 'Adjust SKU-001' }));
    fireEvent.change(screen.getByLabelText('Quantity change'), { target: { value: '5' } });
    fireEvent.click(screen.getByRole('button', { name: 'Post adjustment' }));

    expect(mutate).toHaveBeenCalledWith(
      expect.objectContaining({
        warehouseId: 'wh-1',
        itemId: 'item-1',
        quantityDelta: 5,
        movementType: 'Adjustment',
      }),
      expect.anything(),
    );
  });

  it('opens the adjust dialog from the section action with the active filter', () => {
    api.useSession.mockReturnValue(
      createSession({
        activeModules: [MODULE_CODES.invCore, MODULE_CODES.coreMasterData],
        permissions: [...READ_SESSION.permissions, PERMISSIONS.inventory.stockAdjust],
      }),
    );

    render(<InventoryView />);

    fireEvent.change(screen.getByLabelText('Warehouse filter'), {
      target: { value: 'wh-1' },
    });
    fireEvent.click(screen.getByRole('button', { name: 'Adjust stock' }));

    expect(screen.getByRole('dialog')).toBeInTheDocument();
    expect(screen.getByLabelText('Warehouse')).toHaveValue('wh-1');
  });
});
