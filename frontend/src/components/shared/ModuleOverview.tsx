import { EmptyState } from '@/components/shared/EmptyState';
import { PageHeader } from '@/components/shared/PageHeader';
import { t, type TranslationKey } from '@/lib/i18n';

type Props = {
  titleKey: TranslationKey;
  descriptionKey: TranslationKey;
};

/**
 * Phase 4A module landing surface: standard page header plus the required
 * empty/loading states. Module-specific dashboards land in Phase 4B.
 */
export function ModuleOverview({ titleKey, descriptionKey }: Props) {
  return (
    <div className="space-y-6">
      <PageHeader title={t(titleKey)} description={t(descriptionKey)} />
      <EmptyState title={t('module.empty.title')} description={t('module.empty.body')} />
    </div>
  );
}
