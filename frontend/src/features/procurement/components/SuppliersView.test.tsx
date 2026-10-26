import { render, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import type { Supplier } from '@/features/procurement/api/types';
import { pageOf } from '@/test/fixtures';
import { queryResult } from '@/test/query';

import { SuppliersView } from './SuppliersView';

const api = vi.hoisted(() => ({
  useGetSuppliers: vi.fn(),
}));

vi.mock('@/features/procurement/api/useGetSuppliers', () => ({
  useGetSuppliers: api.useGetSuppliers,
}));

const ACTIVE: Supplier = {
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

const INACTIVE: Supplier = {
  id: 'sup-2',
  code: 'SUP-002',
  name: 'Legacy Trading',
  contactName: null,
  email: null,
  phone: null,
  paymentTermsDays: 14,
  isActive: false,
  createdAt: '2026-10-19T10:00:00Z',
  updatedAt: null,
};

beforeEach(() => {
  api.useGetSuppliers.mockReturnValue(queryResult({ data: pageOf([ACTIVE, INACTIVE]) }));
});

describe('SuppliersView', () => {
  it('renders suppliers with contact details, payment terms and status badges', () => {
    render(<SuppliersView />);

    expect(screen.getByRole('table', { name: 'Suppliers' })).toBeInTheDocument();
    expect(screen.getByText('SUP-001')).toBeInTheDocument();
    expect(screen.getByText('Northwind Supply')).toBeInTheDocument();
    expect(screen.getByText('Dana Reed')).toBeInTheDocument();
    expect(screen.getByText('dana@northwind.test')).toBeInTheDocument();
    expect(screen.getByText('+62 21 555 0100')).toBeInTheDocument();
    expect(screen.getByText('30 days')).toBeInTheDocument();
    expect(screen.getByText('Active')).toBeInTheDocument();
    expect(screen.getByText('Inactive')).toBeInTheDocument();
    expect(screen.getAllByText('Not recorded')).toHaveLength(3);
  });

  it('renders the empty state when the vendor directory is empty', () => {
    api.useGetSuppliers.mockReturnValue(queryResult({ data: pageOf([]) }));

    render(<SuppliersView />);

    expect(screen.getByText('No suppliers yet')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Refresh suppliers' })).toBeInTheDocument();
  });

  it('renders the loading skeleton while the list loads', () => {
    api.useGetSuppliers.mockReturnValue(queryResult({ isLoading: true }));

    render(<SuppliersView />);

    expect(screen.getByRole('status')).toHaveTextContent('Loading content');
  });
});
