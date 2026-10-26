/** Mirrors `WorkOrderStatus` (Manufacturing module). */
export type WorkOrderStatus = 'Draft' | 'Released' | 'InProgress' | 'Completed' | 'Cancelled';

/** Mirrors `WorkOrderDto` (Manufacturing module). */
export interface WorkOrder {
  id: string;
  orderNumber: string;
  productItemId: string;
  billOfMaterialId: string;
  billOfMaterialCode: string;
  status: WorkOrderStatus;
  quantityPlanned: string;
  quantityProduced: string;
  dueDate: string | null;
  notes: string | null;
  createdAt: string;
  updatedAt: string | null;
}

/** Mirrors `BillOfMaterialComponentDto` (Manufacturing module). */
export interface BillOfMaterialComponent {
  id: string;
  itemId: string;
  quantityPerUnit: string;
}

/** Mirrors `BillOfMaterialDto` (Manufacturing module). */
export interface BillOfMaterial {
  id: string;
  code: string;
  name: string;
  productItemId: string;
  version: number;
  isActive: boolean;
  notes: string | null;
  components: BillOfMaterialComponent[];
  createdAt: string;
  updatedAt: string | null;
}

/** Request body for `POST /api/v1/manufacturing/work-orders`. */
export interface CreateWorkOrderRequest {
  orderNumber: string;
  billOfMaterialId: string;
  quantityPlanned: number;
  dueDate: string | null;
  notes: string | null;
}
