import { useCallback, useSyncExternalStore } from 'react';

const STORAGE_KEY = 'omni.sidebar.collapsed';

const listeners = new Set<() => void>();

function readPreference(): boolean {
  try {
    return window.localStorage.getItem(STORAGE_KEY) === 'true';
  } catch (error) {
    console.warn('Sidebar preference could not be read.', error);
    return false;
  }
}

function notify(): void {
  for (const listener of listeners) {
    listener();
  }
}

function subscribe(listener: () => void): () => void {
  listeners.add(listener);
  window.addEventListener('storage', listener);

  return () => {
    listeners.delete(listener);
    window.removeEventListener('storage', listener);
  };
}

function getServerSnapshot(): boolean {
  return false;
}

/**
 * Persists the collapsed rail preference per browser (DESIGN.md §3.2 rule 5).
 * This is a UI preference only — never session data or tokens.
 *
 * `useSyncExternalStore` keeps the server render (always expanded) and the
 * client render consistent without a setState-in-effect cascade.
 */
export function useSidebarPreference(): {
  isCollapsed: boolean;
  toggleCollapsed: () => void;
} {
  const isCollapsed = useSyncExternalStore(subscribe, readPreference, getServerSnapshot);

  const toggleCollapsed = useCallback(() => {
    const next = !readPreference();

    try {
      window.localStorage.setItem(STORAGE_KEY, String(next));
    } catch (error) {
      console.warn('Sidebar preference could not be persisted.', error);
    }

    notify();
  }, []);

  return { isCollapsed, toggleCollapsed };
}
