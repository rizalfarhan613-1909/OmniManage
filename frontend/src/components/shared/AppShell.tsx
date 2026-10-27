'use client';

import { useMemo, useState, type ReactNode } from 'react';

import { useI18n } from '@/components/providers/LanguageProvider';
import { useSession } from '@/components/providers/SessionProvider';
import { DynamicSidebar } from '@/components/shared/DynamicSidebar';
import { Topbar } from '@/components/shared/Topbar';
import { NAVIGATION_REGISTRY } from '@/config/navigation.registry';
import { buildSidebar } from '@/lib/navigation/buildSidebar';
import { useSidebarPreference } from '@/lib/navigation/useSidebarPreference';

type Props = {
  children: ReactNode;
};

/** Main app shell: charcoal sidebar (264px ↔ 72px) + topbar + content area. */
export function AppShell({ children }: Props) {
  const { t } = useI18n();
  const session = useSession();
  const { isCollapsed, toggleCollapsed } = useSidebarPreference();
  const [isMobileOpen, setIsMobileOpen] = useState(false);

  const items = useMemo(
    () =>
      buildSidebar(
        NAVIGATION_REGISTRY,
        session.activeModules,
        session.permissions,
        session.user.roles,
      ),
    [session],
  );

  return (
    <div className="flex min-h-dvh bg-bg">
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:absolute focus:left-4 focus:top-4 focus:z-50 focus:rounded-md focus:bg-surface focus:px-4 focus:py-2 focus:text-sm focus:font-medium focus:text-charcoal focus:shadow-md"
      >
        {t('topbar.skipToContent')}
      </a>

      <DynamicSidebar
        items={items}
        isCollapsed={isCollapsed}
        isMobileOpen={isMobileOpen}
        onCloseMobile={() => setIsMobileOpen(false)}
        onToggleCollapsed={toggleCollapsed}
      />

      <div className="flex min-w-0 flex-1 flex-col">
        <Topbar onOpenMobileNav={() => setIsMobileOpen(true)} />
        <main id="main-content" tabIndex={-1} className="flex-1 p-4 lg:p-6">
          <div className="mx-auto w-full max-w-screen-2xl">{children}</div>
        </main>
      </div>
    </div>
  );
}
