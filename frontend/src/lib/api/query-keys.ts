/** Stable query-key roots shared across feature boundaries (Rules.md §4.3). */
export const QUERY_KEY_ROOTS = {
  dashboard: 'dashboard',
  masterData: 'masterdata',
  inventory: 'inventory',
  procurement: 'procurement',
  manufacturing: 'manufacturing',
  sales: 'sales',
  finance: 'finance',
  admin: 'admin',
  hris: 'hris',
  projects: 'projects',
  assets: 'assets',
} as const;

export interface QueryHookOptions {
  /** Set false to keep a query dormant (e.g. module OFF or no permission). */
  enabled?: boolean;
}
