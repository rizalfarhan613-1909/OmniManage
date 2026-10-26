import { fireEvent, render, screen } from '@testing-library/react';
import type { ReactNode } from 'react';
import { describe, expect, it, vi } from 'vitest';

import type { SortState } from '@/lib/api/pagination';

import { DataTable, type DataTableColumn, type DataTablePagination } from './DataTable';

type Row = {
  id: string;
  name: string;
  amount: string;
};

const ROWS: Row[] = [
  { id: '1', name: 'Alpha', amount: '1000' },
  { id: '2', name: 'Beta', amount: '2000' },
];

const COLUMNS: DataTableColumn<Row>[] = [
  { id: 'name', header: 'Name', sortable: true, cell: (row) => row.name },
  {
    id: 'amount',
    header: 'Amount',
    sortable: true,
    align: 'right',
    cell: (row) => row.amount,
  },
];

type TableOverrides = {
  data?: readonly Row[];
  isLoading?: boolean;
  sort?: SortState | null;
  onSortChange?: (sort: SortState) => void;
  pagination?: DataTablePagination;
  onPageChange?: (page: number) => void;
  emptyState?: ReactNode;
};

function renderTable(overrides: TableOverrides = {}) {
  const { data = ROWS, ...rest } = overrides;

  return render(
    <DataTable
      data={data}
      columns={COLUMNS}
      getRowId={(row) => row.id}
      caption="Test records"
      {...rest}
    />,
  );
}

describe('DataTable', () => {
  it('renders rows with an accessible caption and row ids', () => {
    renderTable();

    expect(screen.getByRole('table', { name: 'Test records' })).toBeInTheDocument();
    expect(screen.getByRole('cell', { name: 'Alpha' })).toBeInTheDocument();
    expect(screen.getByRole('cell', { name: 'Beta' })).toBeInTheDocument();
  });

  it('exposes aria-sort and requests the next sort direction', () => {
    const onSortChange = vi.fn();

    renderTable({
      sort: { field: 'name', direction: 'asc' },
      onSortChange,
    });

    const headers = screen.getAllByRole('columnheader');

    expect(headers[0]).toHaveAttribute('aria-sort', 'ascending');
    expect(headers[1]).toHaveAttribute('aria-sort', 'none');

    fireEvent.click(screen.getByRole('button', { name: 'Sort by Name descending' }));

    expect(onSortChange).toHaveBeenCalledWith({ field: 'name', direction: 'desc' });
  });

  it('sorts an unsorted column ascending and marks the active direction', () => {
    const onSortChange = vi.fn();

    renderTable({
      sort: { field: 'name', direction: 'desc' },
      onSortChange,
    });

    const headers = screen.getAllByRole('columnheader');

    expect(headers[0]).toHaveAttribute('aria-sort', 'descending');

    fireEvent.click(screen.getByRole('button', { name: 'Sort by Amount ascending' }));

    expect(onSortChange).toHaveBeenCalledWith({ field: 'amount', direction: 'asc' });
  });

  it('renders server pagination metadata and emits page changes', () => {
    const onPageChange = vi.fn();

    renderTable({
      pagination: { page: 2, pageSize: 10, totalItems: 25, totalPages: 3 },
      onPageChange,
    });

    expect(screen.getByText(/Showing 11.20 of 25/)).toBeInTheDocument();
    expect(screen.getByText('Page 2 of 3')).toBeInTheDocument();

    fireEvent.click(screen.getByRole('button', { name: 'Previous' }));
    expect(onPageChange).toHaveBeenLastCalledWith(1);

    fireEvent.click(screen.getByRole('button', { name: 'Next' }));
    expect(onPageChange).toHaveBeenLastCalledWith(3);
  });

  it('disables pagination controls at the boundaries', () => {
    renderTable({
      pagination: { page: 1, pageSize: 10, totalItems: 10, totalPages: 1 },
      onPageChange: vi.fn(),
    });

    expect(screen.getByRole('button', { name: 'Previous' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Next' })).toBeDisabled();
  });

  it('delegates empty content to the supplied state', () => {
    renderTable({ data: [], emptyState: <p>Nothing to review</p> });

    expect(screen.getByText('Nothing to review')).toBeInTheDocument();
  });

  it('shows the skeleton instead of stale rows while refetching a new page', () => {
    renderTable({ isLoading: true });

    expect(screen.getByRole('status')).toHaveTextContent('Loading content');
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });
});
