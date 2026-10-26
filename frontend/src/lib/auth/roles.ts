import { SYSTEM_ROLES } from '@/config/roles';

/** Roles allowed to open the administrative consoles (Rules.md §10, Security.md §3.4). */
export const ADMIN_CONSOLE_ROLES = [
  SYSTEM_ROLES.tenantAdmin,
  SYSTEM_ROLES.systemSuperadmin,
] as const;

export type AdminConsoleRole = (typeof ADMIN_CONSOLE_ROLES)[number];

export function isSystemSuperadmin(roles: readonly string[]): boolean {
  return roles.includes(SYSTEM_ROLES.systemSuperadmin);
}

/**
 * Role gate for `/admin/**`.
 *
 * Client-side checks shape the experience only; the backend repeats every
 * authorization decision (Security.md §3.3).
 */
export function hasAdminConsoleAccess(roles: readonly string[]): boolean {
  return ADMIN_CONSOLE_ROLES.some((role) => roles.includes(role));
}
