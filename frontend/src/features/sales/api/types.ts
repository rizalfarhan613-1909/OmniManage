/** Mirrors `SalesOrderDto` / `SalesOrderLineDto` (Sales module). */
export type SalesOrderStatus = 'Draft' | 'Confirmed' | 'Cancelled';

export interface SalesOrderLine {
  id: string;
  itemId: string;
  quantity: string;
  unitPrice: string;
  lineTotal: string;
}

export interface SalesOrder {
  id: string;
  orderNumber: string;
  customerId: string;
  status: SalesOrderStatus;
  totalAmount: string;
  lines: SalesOrderLine[];
  createdAt: string;
  updatedAt: string | null;
}
