/**
 * Mirror of the backend contract `OmniManage.BuildingBlocks.Contracts.ModuleCodes`.
 * Module codes MUST NOT be written as string literals in UI code (Rules.md §16.10).
 */
export const MODULE_CODES = {
  platformCore: 'PLATFORM_CORE',
  coreMasterData: 'CORE_MASTERDATA',
  finGl: 'FIN_GL',
  finAr: 'FIN_AR',
  finAp: 'FIN_AP',
  procCore: 'PROC_CORE',
  salesCore: 'SALES_CORE',
  invCore: 'INV_CORE',
  mfgCore: 'MFG_CORE',
  hrisCore: 'HRIS_CORE',
  hrisAttendance: 'HRIS_ATTENDANCE',
  hrisPayroll: 'HRIS_PAYROLL',
  hrisEss: 'HRIS_ESS',
  crmCore: 'CRM_CORE',
  assetCore: 'ASSET_CORE',
  projectCore: 'PROJECT_CORE',
} as const;

export type ModuleCode = (typeof MODULE_CODES)[keyof typeof MODULE_CODES];
