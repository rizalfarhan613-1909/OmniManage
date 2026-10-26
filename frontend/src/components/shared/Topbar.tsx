'use client';

import { Building2, ChevronDown, ChevronRight, LogOut, Menu } from 'lucide-react';
import { usePathname, useRouter } from 'next/navigation';
import { useEffect, useRef, useState } from 'react';

import { useSession } from '@/components/providers/SessionProvider';
import { NAVIGATION_REGISTRY, type NavItem } from '@/config/navigation.registry';
import { apiClient } from '@/lib/api/client';
import { BFF_ROUTES } from '@/lib/api/routes';
import { t, type TranslationKey } from '@/lib/i18n';
import { isNavItemActive } from '@/lib/navigation/buildSidebar';
import { cn } from '@/lib/utils/cn';
import { getInitials } from '@/lib/utils/initials';

function findActiveLabelKey(items: readonly NavItem[], pathname: string): TranslationKey | null {
  for (const item of items) {
    if (item.href && isNavItemActive(item, pathname)) {
      return item.labelKey;
    }

    if (item.children) {
      const childKey = findActiveLabelKey(item.children, pathname);

      if (childKey) {
        return childKey;
      }
    }
  }

  return null;
}

type Props = {
  onOpenMobileNav: () => void;
};

export function Topbar({ onOpenMobileNav }: Props) {
  const session = useSession();
  const pathname = usePathname();
  const router = useRouter();
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [isSigningOut, setIsSigningOut] = useState(false);
  const menuButtonRef = useRef<HTMLButtonElement>(null);
  const menuRef = useRef<HTMLDivElement>(null);

  const activeLabelKey = findActiveLabelKey(NAVIGATION_REGISTRY, pathname);
  const pageTitle = activeLabelKey ? t(activeLabelKey) : t('app.name');

  useEffect(() => {
    if (!isMenuOpen) {
      return;
    }

    const handlePointerDown = (event: MouseEvent) => {
      const target = event.target as Node;

      if (!menuRef.current?.contains(target) && !menuButtonRef.current?.contains(target)) {
        setIsMenuOpen(false);
      }
    };

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        setIsMenuOpen(false);
        menuButtonRef.current?.focus();
        return;
      }

      const isNavigationKey =
        event.key === 'ArrowDown' ||
        event.key === 'ArrowUp' ||
        event.key === 'Home' ||
        event.key === 'End';

      if (!isNavigationKey) {
        return;
      }

      const menuItems = Array.from(
        menuRef.current?.querySelectorAll<HTMLButtonElement>('[role="menuitem"]') ?? [],
      );

      if (menuItems.length === 0) {
        return;
      }

      event.preventDefault();

      const currentIndex = menuItems.indexOf(document.activeElement as HTMLButtonElement);
      let nextIndex = 0;

      if (event.key === 'ArrowDown') {
        nextIndex = currentIndex < 0 ? 0 : (currentIndex + 1) % menuItems.length;
      } else if (event.key === 'ArrowUp') {
        nextIndex =
          currentIndex < 0
            ? menuItems.length - 1
            : (currentIndex - 1 + menuItems.length) % menuItems.length;
      } else if (event.key === 'End') {
        nextIndex = menuItems.length - 1;
      }

      menuItems[nextIndex]?.focus();
    };

    document.addEventListener('mousedown', handlePointerDown);
    document.addEventListener('keydown', handleKeyDown);
    menuRef.current?.querySelector<HTMLButtonElement>('[role="menuitem"]')?.focus();

    return () => {
      document.removeEventListener('mousedown', handlePointerDown);
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [isMenuOpen]);

  const handleSignOut = async () => {
    setIsSigningOut(true);

    try {
      await apiClient.post(BFF_ROUTES.logout, undefined, { skipAuthRefresh: true });
    } catch (error) {
      // The local session is cleared by the BFF regardless; redirect anyway so
      // the user is never stranded on an authenticated page.
      console.warn('Logout request failed; redirecting to login.', error);
    } finally {
      router.replace('/login');
      router.refresh();
    }
  };

  return (
    <header className="bg-surface/95 sticky top-0 z-20 flex h-topbar shrink-0 items-center gap-3 border-b border-sage px-4 backdrop-blur">
      <button
        type="button"
        onClick={onOpenMobileNav}
        aria-label={t('nav.openMenu')}
        aria-controls="app-sidebar"
        className="rounded-md p-2 text-charcoal hover:bg-sage-light focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary md:hidden"
      >
        <Menu className="h-5 w-5" aria-hidden="true" />
      </button>

      <div className="flex min-w-0 items-center gap-2">
        <span className="hidden items-center gap-1.5 rounded-md bg-sage-light px-2 py-1 text-xs font-medium text-charcoal sm:flex">
          <Building2 className="h-3.5 w-3.5" aria-hidden="true" />
          <span className="max-w-[12rem] truncate" title={session.tenant.name}>
            {session.tenant.name}
          </span>
        </span>
        <ChevronRight className="hidden h-4 w-4 text-charcoal-light sm:block" aria-hidden="true" />
        <p className="truncate text-sm font-semibold text-charcoal">{pageTitle}</p>
      </div>

      <div className="relative ml-auto flex items-center gap-2">
        <button
          ref={menuButtonRef}
          type="button"
          onClick={() => setIsMenuOpen((open) => !open)}
          aria-haspopup="menu"
          aria-expanded={isMenuOpen}
          aria-controls="account-menu"
          aria-label={t('topbar.openAccountMenu')}
          className="flex items-center gap-2 rounded-md border border-transparent px-2 py-1.5 transition-colors duration-300 hover:border-sage hover:bg-sage-light focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary"
        >
          <span
            aria-hidden="true"
            className="flex h-8 w-8 items-center justify-center rounded-full bg-mint-accent text-xs font-bold text-charcoal"
          >
            {getInitials(session.user.email)}
          </span>
          <span className="hidden max-w-[14rem] truncate text-sm text-charcoal lg:block">
            {session.user.email}
          </span>
          <ChevronDown
            className={cn(
              'h-4 w-4 text-charcoal-light transition-transform duration-300',
              isMenuOpen && 'rotate-180',
            )}
            aria-hidden="true"
          />
        </button>

        {isMenuOpen ? (
          <div
            ref={menuRef}
            id="account-menu"
            role="menu"
            aria-label={t('topbar.accountMenu')}
            className="absolute right-0 top-full z-30 mt-2 w-72 overflow-hidden rounded-lg border border-sage bg-surface shadow-md"
          >
            <div className="border-b border-sage px-4 py-3">
              <p className="text-xs uppercase tracking-wide text-charcoal-light">
                {t('topbar.signedInAs')}
              </p>
              <p className="mt-1 truncate text-sm font-medium text-charcoal">
                {session.user.email}
              </p>
              <p className="mt-1 truncate text-xs text-charcoal-light">
                {t('topbar.tenant')}: {session.tenant.name} ({session.tenant.code})
              </p>
            </div>
            <div className="p-1">
              <button
                type="button"
                role="menuitem"
                onClick={() => void handleSignOut()}
                disabled={isSigningOut}
                className="flex w-full items-center gap-2 rounded-md px-3 py-2 text-left text-sm text-charcoal transition-colors duration-300 hover:bg-sage-light focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary disabled:cursor-not-allowed disabled:opacity-60"
              >
                <LogOut className="h-4 w-4" aria-hidden="true" />
                {isSigningOut ? t('topbar.signingOut') : t('topbar.signOut')}
              </button>
            </div>
          </div>
        ) : null}
      </div>
    </header>
  );
}
