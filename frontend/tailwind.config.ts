import type { Config } from 'tailwindcss';

/**
 * OmniManage design tokens — DESIGN.md §2.2.
 * Components must use these tokens and never hard-code hex values.
 */
const config: Config = {
  darkMode: ['class'],
  content: ['./src/**/*.{ts,tsx,mdx}'],
  theme: {
    extend: {
      colors: {
        teal: {
          primary: '#14D6E4',
          hover: '#0EB8C4',
        },
        mint: {
          accent: '#8AFFC9',
        },
        charcoal: {
          DEFAULT: '#4A3A3A',
          light: '#6B7C75',
        },
        sage: {
          DEFAULT: '#C9DAD3',
          light: '#EAF1EE',
        },
        primary: {
          50: 'var(--color-primary-50)',
          500: 'var(--color-primary-500)',
          600: 'var(--color-primary-600)',
          700: 'var(--color-primary-700)',
        },
        surface: 'var(--color-surface)',
        bg: 'var(--color-bg)',
        border: 'var(--color-border)',
        success: 'var(--color-success)',
        warning: 'var(--color-warning)',
        danger: 'var(--color-danger)',
        info: 'var(--color-info)',
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
        mono: ['JetBrains Mono', 'monospace'],
      },
      fontSize: {
        kpi: ['2rem', { lineHeight: '2.5rem', fontWeight: '700' }],
      },
      borderRadius: {
        sm: 'var(--radius-sm)',
        md: 'var(--radius-md)',
        lg: 'var(--radius-lg)',
      },
      boxShadow: {
        sm: 'var(--shadow-sm)',
        md: 'var(--shadow-md)',
      },
      width: {
        sidebar: 'var(--sidebar-w)',
        'sidebar-collapsed': 'var(--sidebar-w-collapsed)',
      },
      height: {
        topbar: 'var(--topbar-h)',
      },
      transitionTimingFunction: {
        spring: 'cubic-bezier(0.34, 1.56, 0.64, 1)',
      },
    },
  },
  plugins: [],
};

export default config;
