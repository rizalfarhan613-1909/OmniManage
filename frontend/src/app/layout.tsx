import type { Metadata } from 'next';
import type { ReactNode } from 'react';

import '@/styles/tokens.css';
import './globals.css';

import { t } from '@/lib/i18n';

export const metadata: Metadata = {
  title: {
    default: t('app.name'),
    template: `%s · ${t('app.name')}`,
  },
  description: t('app.description'),
  applicationName: t('app.name'),
  robots: { index: false, follow: false },
};

type Props = {
  children: ReactNode;
};

export default function RootLayout({ children }: Props) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
