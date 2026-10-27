import type { Metadata } from 'next';

import { LandingView } from '@/features/marketing/components/LandingView';
import { translate } from '@/lib/i18n';
import { getRequestLocale } from '@/lib/i18n/server';

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();

  return {
    title: translate(locale, 'landing.meta.title'),
    description: translate(locale, 'landing.meta.description'),
    robots: { index: true, follow: true },
  };
}

export default function LandingPage() {
  return <LandingView />;
}
