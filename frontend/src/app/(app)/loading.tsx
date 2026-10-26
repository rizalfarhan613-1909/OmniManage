import { t } from '@/lib/i18n';

export default function AppLoading() {
  return (
    <div className="space-y-6" aria-busy="true" aria-label={t('states.loading')}>
      <div className="space-y-3 border-b border-sage pb-5">
        <div className="h-7 w-64 animate-pulse rounded-md bg-sage-light" />
        <div className="h-4 w-full max-w-xl animate-pulse rounded-md bg-sage-light" />
      </div>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {Array.from({ length: 4 }, (_, index) => (
          <div
            key={index}
            className="h-28 animate-pulse rounded-lg border border-sage bg-sage-light"
          />
        ))}
      </div>
      <div className="h-72 animate-pulse rounded-lg border border-sage bg-sage-light" />
    </div>
  );
}
