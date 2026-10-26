'use client';

import { PanelLeftClose, PanelLeftOpen, X } from 'lucide-react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useEffect, type ReactNode } from 'react';

import type { NavItem } from '@/config/navigation.registry';
import { t } from '@/lib/i18n';
import { isNavItemActive } from '@/lib/navigation/buildSidebar';
import { cn } from '@/lib/utils/cn';

type Props = {
  items: readonly NavItem[];
  isCollapsed: boolean;
  isMobileOpen: boolean;
  onCloseMobile: () => void;
  onToggleCollapsed: () => void;
};

/**
 * Renders ONLY the items produced by `buildSidebar`. The component itself
 * never decides visibility — filtering is the single responsibility of the
 * navigation utility (Rules.md §10.3).
 */
export function DynamicSidebar({
  items,
  isCollapsed,
  isMobileOpen,
  onCloseMobile,
  onToggleCollapsed,
}: Props) {
  const pathname = usePathname();
  const isRail = isCollapsed && !isMobileOpen;

  useEffect(() => {
    if (!isMobileOpen) {
      return;
    }

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        onCloseMobile();
      }
    };

    document.addEventListener('keydown', handleKeyDown);

    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [isMobileOpen, onCloseMobile]);

  const renderItem = (item: NavItem): ReactNode => {
    const label = t(item.labelKey);
    const Icon = item.icon;
    const isActive = isNavItemActive(item, pathname);

    if (item.children && item.children.length > 0) {
      if (isRail) {
        return (
          <li key={item.id}>
            <button
              type="button"
              onClick={onToggleCollapsed}
              title={label}
              aria-label={t('nav.expand')}
              className={cn(
                'flex w-full items-center justify-center rounded-md px-3 py-2 transition-colors duration-300',
                isActive ? 'bg-white/5 text-white' : 'text-sage hover:bg-white/5 hover:text-white',
              )}
            >
              {Icon ? <Icon className="h-5 w-5" aria-hidden="true" /> : null}
            </button>
          </li>
        );
      }

      return (
        <li key={item.id}>
          {item.href ? (
            <Link
              href={item.href}
              onClick={onCloseMobile}
              aria-current={item.href === pathname ? 'page' : undefined}
              className={cn(
                'flex items-center gap-3 rounded-md border-l-2 px-3 py-2 text-sm font-medium transition-colors duration-300',
                isActive
                  ? 'border-teal-primary bg-white/5 text-white'
                  : 'border-transparent text-sage hover:bg-white/5 hover:text-white',
              )}
            >
              {Icon ? <Icon className="h-5 w-5 shrink-0" aria-hidden="true" /> : null}
              <span className="truncate">{label}</span>
            </Link>
          ) : (
            <p
              className={cn(
                'flex items-center gap-3 px-3 pb-1 pt-4 text-xs font-semibold uppercase tracking-wide',
                isActive ? 'text-mint-accent' : 'text-sage/70',
              )}
            >
              {Icon ? <Icon className="h-4 w-4" aria-hidden="true" /> : null}
              {label}
            </p>
          )}
          <ul className="ml-4 mt-1 space-y-1 border-l border-white/10 pl-3">
            {item.children.map(renderItem)}
          </ul>
        </li>
      );
    }

    if (!item.href) {
      return null;
    }

    return (
      <li key={item.id}>
        <Link
          href={item.href}
          onClick={onCloseMobile}
          aria-current={isActive ? 'page' : undefined}
          title={isRail ? label : undefined}
          className={cn(
            'flex items-center gap-3 rounded-md border-l-2 px-3 py-2 text-sm font-medium transition-colors duration-300',
            isActive
              ? 'border-teal-primary bg-white/5 text-white'
              : 'border-transparent text-sage hover:bg-white/5 hover:text-white',
          )}
        >
          {Icon ? <Icon className="h-5 w-5 shrink-0" aria-hidden="true" /> : null}
          <span className={cn('truncate', isRail && 'sr-only')}>{label}</span>
          {isActive && !isRail ? (
            <span className="ml-auto h-1.5 w-1.5 rounded-full bg-mint-accent" aria-hidden="true" />
          ) : null}
        </Link>
      </li>
    );
  };

  return (
    <>
      <div
        aria-hidden="true"
        onClick={onCloseMobile}
        className={cn(
          'fixed inset-0 z-30 bg-charcoal/60 transition-opacity duration-300 md:hidden',
          isMobileOpen ? 'opacity-100' : 'pointer-events-none opacity-0',
        )}
      />

      <aside
        id="app-sidebar"
        aria-label={t('nav.primary')}
        className={cn(
          'fixed inset-y-0 left-0 z-40 flex h-dvh flex-col bg-charcoal text-sage shadow-md transition-all duration-300 ease-in-out md:sticky md:top-0 md:translate-x-0',
          isRail ? 'w-sidebar-collapsed' : 'w-sidebar',
          isMobileOpen ? 'translate-x-0' : '-translate-x-full',
        )}
      >
        <div className="flex h-topbar shrink-0 items-center gap-3 border-b border-white/10 px-4">
          <Link
            href="/dashboard"
            onClick={onCloseMobile}
            className="flex min-w-0 items-center gap-3 rounded-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary"
          >
            <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-md bg-mint-accent text-sm font-bold text-charcoal">
              OM
            </span>
            <span className={cn('truncate text-sm font-semibold text-white', isRail && 'sr-only')}>
              {t('app.name')}
            </span>
          </Link>
          <button
            type="button"
            onClick={onCloseMobile}
            className="ml-auto rounded-md p-2 text-sage hover:bg-white/5 hover:text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary md:hidden"
            aria-label={t('nav.closeMenu')}
          >
            <X className="h-5 w-5" aria-hidden="true" />
          </button>
        </div>

        <nav
          id="app-sidebar-nav"
          aria-label={t('nav.primary')}
          className="flex-1 overflow-y-auto px-2 py-3"
        >
          <ul className="space-y-1">{items.map(renderItem)}</ul>
        </nav>

        <div className="mt-auto shrink-0 border-t border-white/10 p-2">
          <button
            type="button"
            onClick={onToggleCollapsed}
            aria-expanded={!isCollapsed}
            aria-controls="app-sidebar-nav"
            aria-label={isCollapsed ? t('nav.expand') : t('nav.collapse')}
            className="flex w-full items-center gap-3 rounded-md px-3 py-2 text-sm font-medium text-sage transition-colors duration-300 hover:bg-white/5 hover:text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary"
          >
            {isCollapsed ? (
              <PanelLeftOpen className="h-5 w-5 shrink-0" aria-hidden="true" />
            ) : (
              <PanelLeftClose className="h-5 w-5 shrink-0" aria-hidden="true" />
            )}
            <span className={cn('truncate', isCollapsed && 'sr-only')}>{t('nav.collapse')}</span>
          </button>
        </div>
      </aside>
    </>
  );
}
