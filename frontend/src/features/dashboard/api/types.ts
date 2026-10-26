export type ActivityKind =
  'salesConfirmed' | 'salesDraft' | 'invoicePaid' | 'invoiceUnpaid' | 'journalPosted';

export interface ActivityItem {
  id: string;
  kind: ActivityKind;
  reference: string;
  amount: string;
  occurredAt: string;
}

export interface RevenuePoint {
  monthKey: string;
  label: string;
  revenue: string;
  cashFlow: string;
  /** Presentation-only coordinate values; all money math stays in Decimal. */
  revenueValue: number;
  cashFlowValue: number;
}

export interface DashboardAvailability {
  sales: boolean;
  inventory: boolean;
  stockValue: boolean;
  financeAr: boolean;
  financeGl: boolean;
  masterData: boolean;
}

export interface DashboardRecordCounts {
  salesOrders: number;
  arInvoices: number;
  journalEntries: number;
  stockBalances: number;
  items: number;
  warehouses: number;
}

export interface ExecutiveDashboardData {
  available: DashboardAvailability;
  revenueMtd: string;
  revenuePreviousMonth: string;
  revenueChangePercent: string | null;
  openAr: string;
  cashCollected: string;
  stockValue: string;
  revenueSeries: RevenuePoint[];
  activity: ActivityItem[];
  counts: DashboardRecordCounts;
}
