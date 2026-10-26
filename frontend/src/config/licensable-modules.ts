import { MODULE_CODES, type ModuleCode } from '@/config/module-codes';
import type { TranslationKey } from '@/lib/i18n';

/**
 * Licensable modules shown in the Superadmin matrix (DESIGN.md §3.4 / §3.5
 * view 7). `PLATFORM_CORE` and `CORE_MASTERDATA` are always-on and therefore
 * excluded. Module codes mirror the backend `ModuleCodes` constants.
 */
export const LICENSABLE_MODULE_GROUPS = [
  'finance',
  'supplyChain',
  'revenue',
  'manufacturing',
  'people',
] as const;

export type LicensableModuleGroup = (typeof LICENSABLE_MODULE_GROUPS)[number];

export interface LicensableModule {
  code: ModuleCode;
  descriptionKey: TranslationKey;
  group: LicensableModuleGroup;
  /** Hard dependency documented in DESIGN.md §3.4 (Finance sub-modules). */
  dependsOn?: ModuleCode;
}

export const MODULE_GROUP_LABEL_KEYS: Record<LicensableModuleGroup, TranslationKey> = {
  finance: 'admin.licensing.group.finance',
  supplyChain: 'admin.licensing.group.supplyChain',
  revenue: 'admin.licensing.group.revenue',
  manufacturing: 'admin.licensing.group.manufacturing',
  people: 'admin.licensing.group.people',
};

export const LICENSABLE_MODULES: readonly LicensableModule[] = [
  {
    code: MODULE_CODES.finGl,
    descriptionKey: 'admin.licensing.module.finGl.description',
    group: 'finance',
  },
  {
    code: MODULE_CODES.finAr,
    descriptionKey: 'admin.licensing.module.finAr.description',
    group: 'finance',
    dependsOn: MODULE_CODES.finGl,
  },
  {
    code: MODULE_CODES.finAp,
    descriptionKey: 'admin.licensing.module.finAp.description',
    group: 'finance',
    dependsOn: MODULE_CODES.finGl,
  },
  {
    code: MODULE_CODES.invCore,
    descriptionKey: 'admin.licensing.module.invCore.description',
    group: 'supplyChain',
  },
  {
    code: MODULE_CODES.procCore,
    descriptionKey: 'admin.licensing.module.procCore.description',
    group: 'supplyChain',
  },
  {
    code: MODULE_CODES.salesCore,
    descriptionKey: 'admin.licensing.module.salesCore.description',
    group: 'revenue',
  },
  {
    code: MODULE_CODES.crmCore,
    descriptionKey: 'admin.licensing.module.crmCore.description',
    group: 'revenue',
  },
  {
    code: MODULE_CODES.mfgCore,
    descriptionKey: 'admin.licensing.module.mfgCore.description',
    group: 'manufacturing',
  },
  {
    code: MODULE_CODES.hrisPayroll,
    descriptionKey: 'admin.licensing.module.hrisPayroll.description',
    group: 'people',
  },
];

export function modulesByGroup(group: LicensableModuleGroup): readonly LicensableModule[] {
  return LICENSABLE_MODULES.filter((module) => module.group === group);
}
