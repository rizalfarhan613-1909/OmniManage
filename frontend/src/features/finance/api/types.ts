/** Mirrors `ArInvoiceDto` (Finance/AR module). */
export type ArInvoiceStatus = 'Unpaid' | 'Paid' | 'Cancelled';

export interface ArInvoice {
  id: string;
  invoiceNumber: string;
  salesOrderId: string;
  customerId: string;
  totalAmount: string;
  status: ArInvoiceStatus;
  createdAt: string;
}

/** Mirrors `JournalEntryDto` / `JournalEntryLineDto` (Finance/GL module). */
export interface JournalEntryLine {
  glAccountId: string;
  debit: string;
  credit: string;
}

export interface JournalEntry {
  id: string;
  entryNumber: string;
  postedDateUtc: string;
  description: string;
  debitTotal: string;
  creditTotal: string;
  lines: JournalEntryLine[];
  createdAt: string;
}
