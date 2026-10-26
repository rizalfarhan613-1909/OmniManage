/** Mirrors `SupplierDto` (Procurement module). */
export interface Supplier {
  id: string;
  code: string;
  name: string;
  contactName: string | null;
  email: string | null;
  phone: string | null;
  paymentTermsDays: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}

/** Mirrors `PurchaseOrderStatus` (Procurement module). */
export type PurchaseOrderStatus =
  'Draft' | 'Approved' | 'PartiallyReceived' | 'Received' | 'Cancelled';

/** Mirrors `PurchaseOrderLineDto` (Procurement module). */
export interface PurchaseOrderLine {
  id: string;
  itemId: string;
  quantity: string;
  unitPrice: string;
  lineTotal: string;
}

/** Mirrors `PurchaseOrderDto` (Procurement module). */
export interface PurchaseOrder {
  id: string;
  orderNumber: string;
  supplierId: string;
  supplierName: string;
  status: PurchaseOrderStatus;
  totalAmount: string;
  expectedDeliveryDate: string | null;
  notes: string | null;
  lines: PurchaseOrderLine[];
  createdAt: string;
  updatedAt: string | null;
}

/** Request line for `POST /api/v1/procurement/purchase-orders`. */
export interface CreatePurchaseOrderLineRequest {
  itemId: string;
  quantity: number;
  unitPrice: number;
}

/** Request body for `POST /api/v1/procurement/purchase-orders`. */
export interface CreatePurchaseOrderRequest {
  orderNumber: string;
  supplierId: string;
  expectedDeliveryDate: string | null;
  notes: string | null;
  lines: CreatePurchaseOrderLineRequest[];
}
