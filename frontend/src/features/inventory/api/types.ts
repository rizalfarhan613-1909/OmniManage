/** Mirrors `WarehouseDto` (Inventory module). */
export interface Warehouse {
  id: string;
  code: string;
  name: string;
  address: string | null;
  isActive: boolean;
  createdAt: string;
}

/** Mirrors `StockBalanceDto` (Inventory module). */
export interface StockBalance {
  itemId: string;
  warehouseId: string;
  quantityOnHand: string;
}

/** Mirrors `AdjustStockRequest.MovementType` (Inventory module). */
export type MovementType = 'Receive' | 'Issue' | 'Transfer' | 'Adjustment';

/** Request body for `POST /api/v1/inventory/warehouses`. */
export interface CreateWarehouseRequest {
  code: string;
  name: string;
  address: string | null;
}

/** Request body for `POST /api/v1/inventory/stock/adjustments`. */
export interface AdjustStockRequest {
  warehouseId: string;
  itemId: string;
  quantityDelta: number;
  movementType: MovementType;
  referenceNumber: string;
  occurredAtUtc?: string;
}

/** Mirrors `StockEntryDto` (Inventory module). */
export interface StockEntry {
  id: string;
  warehouseId: string;
  itemId: string;
  quantityDelta: string;
  movementType: MovementType;
  referenceNumber: string;
  occurredAtUtc: string;
}
