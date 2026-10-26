import type { NavItem } from '@/config/navigation.registry';

function toSet(values: Iterable<string>): ReadonlySet<string> {
  return values instanceof Set ? values : new Set(values);
}

/**
 * An item is granted when no requirement is declared, or when at least one of
 * the declared codes is held. An empty requirement list means "no restriction".
 */
function isGranted(
  required: string | readonly string[] | undefined,
  granted: ReadonlySet<string>,
): boolean {
  if (required === undefined) {
    return true;
  }

  if (typeof required === 'string') {
    return granted.has(required);
  }

  if (required.length === 0) {
    return true;
  }

  return required.some((code) => granted.has(code));
}

/**
 * Filters the static navigation registry against the session payload.
 *
 * An item is visible only when ALL of the following hold:
 *  1. its required module is active for the tenant (`activeModules`),
 *  2. its optional permission requirement is met (`permissions`),
 *  3. its optional role requirement is met (`roles`).
 *
 * Children are filtered independently. A parent that has no link of its own
 * and whose children are all filtered out is removed as well, so the sidebar
 * never renders an empty group.
 *
 * The registry is never mutated; filtered nodes are shallow copies.
 */
export function buildSidebar(
  registry: readonly NavItem[],
  activeModules: Iterable<string>,
  permissions: Iterable<string>,
  roles: Iterable<string> = [],
): NavItem[] {
  const active = toSet(activeModules);
  const grantedPermissions = toSet(permissions);
  const grantedRoles = toSet(roles);

  const isVisible = (item: NavItem): boolean =>
    active.has(item.moduleCode) &&
    isGranted(item.permission, grantedPermissions) &&
    isGranted(item.roles, grantedRoles);

  const filterItems = (items: readonly NavItem[]): NavItem[] =>
    items
      .filter(isVisible)
      .map((item): NavItem => {
        const children = item.children ? filterItems(item.children) : undefined;

        return children && children.length > 0
          ? { ...item, children }
          : { ...item, children: undefined };
      })
      .filter((item) => item.href !== undefined || (item.children?.length ?? 0) > 0);

  return filterItems(registry);
}

/**
 * True when `pathname` is the item's route or a descendant of it. Parents
 * without an `href` are active when any child is active.
 */
export function isNavItemActive(item: NavItem, pathname: string): boolean {
  if (item.href) {
    return pathname === item.href || pathname.startsWith(`${item.href}/`);
  }

  return item.children?.some((child) => isNavItemActive(child, pathname)) ?? false;
}
