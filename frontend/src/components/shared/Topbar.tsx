'use client';

import {
  Bell,
  Building2,
  ChevronDown,
  Layers,
  LogOut,
  Menu,
  Search,
  ShieldCheck,
} from 'lucide-react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useEffect, useRef, useState } from 'react';

import { Logo } from '@/components/brand/Logo';
import { useI18n } from '@/components/providers/LanguageProvider';
import { useSession } from '@/components/providers/SessionProvider';
import { LanguageSwitcher } from '@/components/shared/LanguageSwitcher';
import { ThemeToggle } from '@/components/shared/ThemeToggle';
import { apiClient } from '@/lib/api/client';
import { BFF_ROUTES } from '@/lib/api/routes';
import { cn } from '@/lib/utils/cn';
import { getInitials } from '@/lib/utils/initials';
import { formatPersonName, formatRoleLabel } from '@/lib/utils/names';

type Props = {
  onOpenMobileNav: () => void;
};

export function Topbar({ onOpenMobileNav }: Props) {
  const session = useSession();
  const router = useRouter();
  const { t } = useI18n();
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [isSigningOut, setIsSigningOut] = useState(false);
  const menuButtonRef = useRef<HTMLButtonElement>(null);
  const menuRef = useRef<HTMLDivElement>(null);
  const searchInputRef = useRef<HTMLInputElement>(null);

  const personName = formatPersonName(session.user.email);
  const roleLabel = formatRoleLabel(session.user.roles[0]);
  const activeModuleCount = session.activeModules.length;

  useEffect(() => {
    const handleShortcut = (event: KeyboardEvent) => {
      if ((event.metaKey || event.ctrlKey) && event.key.toLowerCase() === 'k') {
        event.preventDefault();
        searchInputRef.current?.focus();
      }
    };

    window.addEventListener('keydown', handleShortcut);

    return () => window.removeEventListener('keydown', handleShortcut);
  }, []);

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
      }
    };

    document.addEventListener('mousedown', handlePointerDown);
    document.addEventListener('keydown', handleKeyDown);

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
    <header className="sticky top-0 z-20 flex h-topbar shrink-0 items-center gap-2 border-b border-line bg-surface/95 px-3 backdrop-blur-xl sm:gap-3 sm:px-4">
      <button
        type="button"
        onClick={onOpenMobileNav}
        aria-label={t('nav.openMenu')}
        aria-controls="app-sidebar"
        className="rounded-lg p-2 text-charcoal transition-colors hover:bg-surface-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary md:hidden"
      >
        <Menu className="h-5 w-5" aria-hidden="true" />
      </button>

      <Link
        href="/dashboard"
        aria-label={t('app.name')}
        className="flex items-center rounded-lg focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary md:hidden"
      >
        <Logo size="sm" showWordmark={false} />
      </Link>

      <button
        type="button"
        className="hidden min-w-0 items-center gap-2 rounded-xl border border-line bg-surface px-3 py-2 text-left shadow-sm transition-colors hover:border-cyan/40 hover:bg-cyan/5 sm:flex"
        aria-label={t('topbar.tenantSelector', { tenant: session.tenant.name })}
      >
        <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-surface-muted text-charcoal-strong">
          <Building2 className="h-4 w-4" aria-hidden="true" />
        </span>
        <span className="min-w-0">
          <span className="block max-w-[12rem] truncate text-xs font-semibold text-charcoal-strong">
            {session.tenant.name}
          </span>
          <span className="block truncate font-mono text-[10px] text-charcoal-light">
            {session.tenant.code}
          </span>
        </span>
        <ChevronDown className="h-3.5 w-3.5 shrink-0 text-charcoal-light" aria-hidden="true" />
      </button>

      <div className="relative hidden min-w-0 flex-1 items-center md:flex md:max-w-md">
        <Search
          className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-charcoal-light"
          aria-hidden="true"
        />
        <input
          ref={searchInputRef}
          type="search"
          placeholder={t('common.searchPlaceholder')}
          aria-label={t('common.search')}
          className="h-9 w-full rounded-xl border border-line bg-surface-muted pl-9 pr-16 text-xs text-charcoal outline-none transition-colors placeholder:text-charcoal-light/70 focus:border-cyan focus:bg-surface focus:ring-2 focus:ring-cyan/20"
        />
        <kbd className="pointer-events-none absolute right-2 top-1/2 -translate-y-1/2 rounded-md border border-line bg-surface px-1.5 py-0.5 font-mono text-[9px] font-semibold text-charcoal-light">
          ⌘K
        </kbd>
      </div>

      <div className="ml-auto flex items-center gap-1.5 sm:gap-2">
        <span className="hidden items-center gap-2 rounded-xl border border-line bg-surface px-2.5 py-1.5 text-[10px] font-semibold text-charcoal-light lg:flex">
          <ShieldCheck className="h-3.5 w-3.5 text-emerald-500" aria-hidden="true" />
          <span>
            <span className="block font-mono text-xs font-bold text-charcoal-strong">99.98%</span>
            <span className="block uppercase tracking-[0.08em]">{t('common.operational')}</span>
          </span>
        </span>

        <Link
          href="/admin/licensing"
          className="hidden items-center gap-2 rounded-xl bg-cyan px-2.5 py-1.5 text-[10px] font-semibold text-on-accent transition-colors hover:bg-cyan-deep hover:text-white sm:flex"
        >
          <Layers className="h-3.5 w-3.5" aria-hidden="true" />
          <span>
            <span className="block font-mono text-xs font-bold">{activeModuleCount}</span>
            <span className="block uppercase tracking-[0.08em]">{t('common.activeModules')}</span>
          </span>
        </Link>

        <LanguageSwitcher />
        <ThemeToggle />

        <button
          type="button"
          aria-label={t('common.notifications')}
          className="relative rounded-lg p-2 text-charcoal-light transition-colors hover:bg-surface-muted hover:text-charcoal-strong focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary"
        >
          <Bell className="h-4 w-4" aria-hidden="true" />
          <span className="absolute right-1.5 top-1.5 h-1.5 w-1.5 rounded-full bg-danger" />
        </button>

        <div className="relative flex items-center">
          <button
            ref={menuButtonRef}
            type="button"
            onClick={() => setIsMenuOpen((open) => !open)}
            aria-haspopup="menu"
            aria-expanded={isMenuOpen}
            aria-controls="account-menu"
            aria-label={t('topbar.openAccountMenu')}
            className="flex items-center gap-2 rounded-xl border border-transparent px-1.5 py-1 transition-colors duration-300 hover:border-line hover:bg-surface-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary sm:px-2"
          >
            <span
              aria-hidden="true"
              className="flex h-8 w-8 items-center justify-center rounded-full bg-sidebar text-xs font-bold text-mint-accent"
            >
              {getInitials(session.user.email)}
            </span>
            <span className="hidden max-w-[8rem] text-left lg:block">
              <span className="block truncate text-xs font-semibold text-charcoal-strong">
                {personName}
              </span>
              <span className="block truncate text-[10px] text-charcoal-light">{roleLabel}</span>
            </span>
            <ChevronDown
              className={cn(
                'hidden h-3.5 w-3.5 text-charcoal-light transition-transform duration-300 sm:block',
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
              className="absolute right-0 top-full z-30 mt-2 w-72 overflow-hidden rounded-xl border border-line bg-surface shadow-md"
            >
              <div className="border-b border-line px-4 py-3">
                <p className="text-[10px] font-bold uppercase tracking-[0.12em] text-charcoal-light">
                  {t('topbar.signedInAs')}
                </p>
                <p className="mt-1 truncate text-sm font-semibold text-charcoal-strong">
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
                  className="flex w-full items-center gap-2 rounded-lg px-3 py-2 text-left text-sm text-charcoal transition-colors duration-300 hover:bg-surface-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-teal-primary disabled:cursor-not-allowed disabled:opacity-60"
                >
                  <LogOut className="h-4 w-4" aria-hidden="true" />
                  {isSigningOut ? t('topbar.signingOut') : t('topbar.signOut')}
                </button>
              </div>
            </div>
          ) : null}
        </div>
      </div>
    </header>
  );
}
