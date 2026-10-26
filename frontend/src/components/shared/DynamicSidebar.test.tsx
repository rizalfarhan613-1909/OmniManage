import { render, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { MODULE_CODES } from '@/config/module-codes';
import { NAVIGATION_REGISTRY } from '@/config/navigation.registry';
import { PERMISSIONS } from '@/config/permissions';
import { SYSTEM_ROLES } from '@/config/roles';
import { buildSidebar } from '@/lib/navigation/buildSidebar';

import { DynamicSidebar } from './DynamicSidebar';

const usePathnameMock = vi.hoisted(() => vi.fn<() => string>());

vi.mock('next/navigation', () => ({
  usePathname: usePathnameMock,
}));

const visibleItems = buildSidebar(
  NAVIGATION_REGISTRY,
  [MODULE_CODES.platformCore, MODULE_CODES.salesCore, MODULE_CODES.invCore],
  [PERMISSIONS.sales.orderRead],
  [],
);

function renderSidebar(isCollapsed = false) {
  return render(
    <DynamicSidebar
      items={visibleItems}
      isCollapsed={isCollapsed}
      isMobileOpen={false}
      onCloseMobile={vi.fn()}
      onToggleCollapsed={vi.fn()}
    />,
  );
}

beforeEach(() => {
  usePathnameMock.mockReturnValue('/dashboard');
});

describe('DynamicSidebar', () => {
  it('renders only the items produced by buildSidebar', () => {
    renderSidebar();

    expect(screen.getByRole('link', { name: 'Executive BI' })).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'CRM & Sales' })).toBeInTheDocument();
    expect(screen.queryByRole('link', { name: 'Supply Chain' })).not.toBeInTheDocument();
    expect(screen.queryByRole('link', { name: 'Finance' })).not.toBeInTheDocument();
  });

  it('marks the current route with aria-current', () => {
    usePathnameMock.mockReturnValue('/sales');
    renderSidebar();

    expect(screen.getByRole('link', { name: 'CRM & Sales' })).toHaveAttribute(
      'aria-current',
      'page',
    );
    expect(screen.getByRole('link', { name: 'Executive BI' })).not.toHaveAttribute('aria-current');
  });

  it('keeps accessible names when the rail is collapsed', () => {
    renderSidebar(true);

    expect(screen.getByRole('link', { name: 'Executive BI' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Expand navigation' })).toBeInTheDocument();
  });

  it('renders the primary navigation landmark', () => {
    renderSidebar();

    expect(screen.getByRole('navigation', { name: 'Primary navigation' })).toBeInTheDocument();
  });

  it('renders module sub-routes beneath their parent landing link', () => {
    usePathnameMock.mockReturnValue('/manufacturing/bom');

    render(
      <DynamicSidebar
        items={buildSidebar(
          NAVIGATION_REGISTRY,
          [MODULE_CODES.platformCore, MODULE_CODES.mfgCore],
          [PERMISSIONS.manufacturing.billOfMaterialRead],
          [SYSTEM_ROLES.tenantAdmin],
        )}
        isCollapsed={false}
        isMobileOpen={false}
        onCloseMobile={vi.fn()}
        onToggleCollapsed={vi.fn()}
      />,
    );

    expect(screen.getByRole('link', { name: 'Manufacturing' })).toHaveAttribute(
      'href',
      '/manufacturing',
    );
    expect(screen.getByRole('link', { name: 'Bill of materials' })).toHaveAttribute(
      'href',
      '/manufacturing/bom',
    );
    expect(screen.getByRole('link', { name: 'Bill of materials' })).toHaveAttribute(
      'aria-current',
      'page',
    );
    expect(screen.getByRole('link', { name: 'Manufacturing' })).not.toHaveAttribute('aria-current');
  });
});
