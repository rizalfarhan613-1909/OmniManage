import { translate } from '@/lib/i18n';
import { getRequestLocale } from '@/lib/i18n/server';

export default async function AppLoading() {
  const locale = await getRequestLocale();

  return (
    <div className="space-y-6" aria-busy="true" aria-label={translate(locale, 'states.loading')}>
      <div className="space-y-3 border-b border-line pb-5">
        <div className="h-7 w-64 animate-pulse rounded-md bg-surface-muted" />
        <div className="h-4 w-full max-w-xl animate-pulse rounded-md bg-surface-muted" />
      </div>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {Array.from({ length: 4 }, (_, index) => (
          <div
            key={index}
            className="h-28 animate-pulse rounded-lg border border-line bg-surface-muted"
          />
        ))}
      </div>
      <div className="h-72 animate-pulse rounded-lg border border-line bg-surface-muted" />
    </div>
  );
}
