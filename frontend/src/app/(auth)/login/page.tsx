import type { Metadata } from 'next';
import { redirect } from 'next/navigation';

import { LoginView } from '@/features/auth/components/LoginView';
import { getSession } from '@/lib/auth/session';
import { translate } from '@/lib/i18n';
import { getRequestLocale } from '@/lib/i18n/server';

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();

  return {
    title: translate(locale, 'login.title'),
    description: translate(locale, 'login.accessSubtitle'),
  };
}

export default async function LoginPage() {
  const session = await getSession();

  if (session) {
    redirect('/dashboard');
  }

  return <LoginView />;
}
