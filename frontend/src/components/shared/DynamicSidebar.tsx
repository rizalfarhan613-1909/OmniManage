'use client';

import { PanelLeftClose, PanelLeftOpen, X } from 'lucide-react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { Fragment, useEffect, type ReactNode } from 'react';

import { Logo } from '@/components/brand/Logo';
import { useI18n } from '@/components/providers/LanguageProvider';
import type { NavItem } from '@/config/navigation.registry';
import { isNavItemActive } from '@/lib/navigation/buildSidebar';
import { cn } from '@/lib/utils/cn';

type Props = {
  items: readonly NavItem[];
  isCollapsed: boolean;
  isMobileOpen: boolean;
  onCloseMobile: () => void;
  onToggleCollapsed: () => void;
};

const CORE_IDS = new Set(['dashboard']);
const UTILITY_IDS = new Set(['admin']);

function SidebarGroupLabel({ label, count }: { label: string; count?: number }) {
  const { t } = useI18n();

  return (
    <li className="flex items-center justify-between gap-2 px-3 pb-1 pt-4 text-[9px] font-bold uppercase tracking-[0.18em] text-sidebar-muted/45 first:pt-0">
      <span>{label}</span>
      {count !== undefined ? (
        <span className="rounded-full bg-mint-accent/15 px-1.5 py-0.5 font-mono text-[9px] font-bold text-mint-accent">
          {t('dashboard.modules.active', { count })}
        </span>
      ) : null}
    </li>
  );
}

/**
 * Renders ONLY the items produced by `buildSidebar`. The component itself
 * never decides visibility - filtering is the single responsibility of the
 * navigation utility (Rules.md 10.3).
 */
export function DynamicSidebar({
  items,
  isCollapsed,
  isMobileOpen,
  onCloseMobile,
  onToggleCollapsed,
}: Props) {
  const pathname = usePathname();
  const { t } = useI18n();
  const isRail = isCollapsed && !isMobileOpen;
  const firstModuleIndex = items.findIndex(
    (item) => !CORE_IDS.has(item.id) && !UTILITY_IDS.has(item.id),
  );
  const utilityIndex = items.findIndex((item) => UTILITY_IDS.has(item.id));
  const licensedModuleCount =
    utilityIndex >= 0
      ? items.slice(firstModuleIndex >= 0 ? firstModuleIndex : 0, utilityIndex).length
      : items.filter((item) => !CORE_IDS.has(item.id) && !UTILITY_IDS.has(item.id)).length;

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
                'flex w-full items-center justify-center rounded-lg px-3 py-2 transition-colors duration-300',
                isActive
                  ? 'bg-white/10 text-white'
                  : 'text-sidebar-muted/80 hover:bg-white/5 hover:text-white',
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
                'flex items-center gap-3 rounded-lg border-l-2 px-3 py-2 text-sm font-medium transition-colors duration-300',
                isActive
                  ? 'border-cyan bg-white/10 text-white'
                  : 'border-transparent text-sidebar-muted/80 hover:bg-white/5 hover:text-white',
              )}
            >
              {Icon ? <Icon className="h-5 w-5 shrink-0" aria-hidden="true" /> : null}
              <span className="truncate">{label}</span>
            </Link>
          ) : (
            <p
              className={cn(
                'flex items-center gap-3 px-3 pb-1 pt-4 text-xs font-semibold uppercase tracking-wide',
                isActive ? 'text-mint-accent' : 'text-sidebar-muted/60',
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
            'flex items-center gap-3 rounded-lg border-l-2 px-3 py-2 text-sm font-medium transition-colors duration-300',
            isActive
              ? 'border-cyan bg-white/10 text-white'
              : 'border-transparent text-sidebar-muted/80 hover:bg-white/5 hover:text-white',
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
          'fixed inset-0 z-30 bg-sidebar/70 backdrop-blur-sm transition-opacity duration-300 md:hidden',
          isMobileOpen ? 'opacity-100' : 'pointer-events-none opacity-0',
        )}
      />

      <aside
        id="app-sidebar"
        aria-label={t('nav.primary')}
        className={cn(
          'fixed inset-y-0 left-0 z-40 flex h-dvh flex-col bg-sidebar text-sidebar-muted shadow-md transition-all duration-300 ease-in-out md:sticky md:top-0 md:translate-x-0',
          isRail ? 'w-sidebar-collapsed' : 'w-sidebar',
          isMobileOpen ? 'translate-x-0' : '-translate-x-full',
        )}
      >
        <div className="flex h-topbar shrink-0 items-center gap-3 border-b border-white/10 px-4">
          <Link
            href="/dashboard"
            onClick={onCloseMobile}
            aria-label={t('app.name')}
            className={cn(
              'flex min-w-0 items-center gap-3 rounded-lg focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary',
              isRail && 'w-full justify-center',
            )}
          >
            <Logo
              size="sm"
              theme="dark"
              showWordmark={!isRail}
              badge={isRail ? undefined : 'ERP'}
            />
          </Link>
          <button
            type="button"
            onClick={onCloseMobile}
            className="ml-auto rounded-lg p-2 text-sidebar-muted transition-colors hover:bg-white/5 hover:text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary md:hidden"
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
          <ul className="space-y-0.5">
            {items.map((item, index) => (
              <Fragment key={item.id}>
                {!isRail && index === 0 ? <SidebarGroupLabel label={t('nav.group.core')} /> : null}
                {!isRail && index === firstModuleIndex && firstModuleIndex > 0 ? (
                  <SidebarGroupLabel label={t('nav.group.modules')} count={licensedModuleCount} />
                ) : null}
                {!isRail && index === utilityIndex && utilityIndex > 0 ? (
                  <SidebarGroupLabel label={t('nav.group.utilities')} />
                ) : null}
                {renderItem(item)}
              </Fragment>
            ))}
          </ul>
        </nav>

        <div className="mt-auto shrink-0 border-t border-white/10 p-2">
          {!isRail ? (
            <div className="mb-2 flex items-center gap-2 rounded-lg border border-white/10 bg-white/[0.04] px-3 py-2">
              <span className="relative flex h-1.5 w-1.5 shrink-0">
                <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-mint-accent opacity-60" />
                <span className="relative inline-flex h-1.5 w-1.5 rounded-full bg-mint-accent" />
              </span>
              <span className="truncate text-[10px] font-semibold uppercase tracking-[0.12em] text-sidebar-muted/70">
                {t('common.version')}
              </span>
            </div>
          ) : null}
          <button
            type="button"
            onClick={onToggleCollapsed}
            aria-expanded={!isCollapsed}
            aria-controls="app-sidebar-nav"
            aria-label={isCollapsed ? t('nav.expand') : t('nav.collapse')}
            className="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-sidebar-muted/80 transition-colors duration-300 hover:bg-white/5 hover:text-white focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary"
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
