import type { Metadata } from 'next';
import Script from 'next/script';
import type { ReactNode } from 'react';

import '@/styles/tokens.css';
import './globals.css';

import { LanguageProvider } from '@/components/providers/LanguageProvider';
import { ThemeProvider } from '@/components/providers/ThemeProvider';
import { getRequestLocale } from '@/lib/i18n/server';
import { translate } from '@/lib/i18n';
import { THEME_INIT_SCRIPT } from '@/lib/theme';

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();

  return {
    title: {
      default: translate(locale, 'app.name'),
      template: `%s · ${translate(locale, 'app.name')}`,
    },
    description: translate(locale, 'app.description'),
    applicationName: translate(locale, 'app.name'),
    robots: { index: false, follow: false },
  };
}

type Props = {
  children: ReactNode;
};

export default async function RootLayout({ children }: Props) {
  const locale = await getRequestLocale();

  return (
    <html lang={locale} suppressHydrationWarning>
      <body>
        <Script id="omnimanage-theme-init" strategy="beforeInteractive">
          {THEME_INIT_SCRIPT}
        </Script>
        <ThemeProvider>
          <LanguageProvider initialLocale={locale}>{children}</LanguageProvider>
        </ThemeProvider>
      </body>
    </html>
  );
}
