import { MODULE_CODES, type ModuleCode } from '@/config/module-codes';
import type { TranslationKey } from '@/lib/i18n';

/** Human-readable labels for module codes surfaced in dashboards/consoles. */
export const MODULE_LABELS: Record<ModuleCode, TranslationKey> = {
  [MODULE_CODES.platformCore]: 'module.platformCore',
  [MODULE_CODES.coreMasterData]: 'module.coreMasterData',
  [MODULE_CODES.finGl]: 'module.finGl',
  [MODULE_CODES.finAr]: 'module.finAr',
  [MODULE_CODES.finAp]: 'module.finAp',
  [MODULE_CODES.procCore]: 'module.procCore',
  [MODULE_CODES.salesCore]: 'module.salesCore',
  [MODULE_CODES.invCore]: 'module.invCore',
  [MODULE_CODES.mfgCore]: 'module.mfgCore',
  [MODULE_CODES.hrisCore]: 'module.hrisCore',
  [MODULE_CODES.hrisAttendance]: 'module.hrisAttendance',
  [MODULE_CODES.hrisPayroll]: 'module.hrisPayroll',
  [MODULE_CODES.hrisEss]: 'module.hrisEss',
  [MODULE_CODES.crmCore]: 'module.crmCore',
  [MODULE_CODES.assetCore]: 'module.assetCore',
  [MODULE_CODES.projectCore]: 'module.projectCore',
};

export function getModuleLabel(moduleCode: string): TranslationKey {
  return MODULE_LABELS[moduleCode as ModuleCode] ?? 'module.unknown';
}
