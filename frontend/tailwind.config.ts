import type { Config } from 'tailwindcss';

/**
 * OmniManage design tokens - Google Stitch v4.18.2.
 * Components must use these tokens and never hard-code hex values.
 *
 * Adaptive tokens are backed by CSS variables (RGB triplets) declared in
 * `src/styles/tokens.css`, so every utility responds to the `.dark` class.
 */
const config: Config = {
  darkMode: ['class'],
  content: ['./src/**/*.{ts,tsx,mdx}'],
  theme: {
    extend: {
      colors: {
        teal: {
          primary: '#00f2fe',
          hover: '#00d6e4',
        },
        mint: {
          accent: '#8affc9',
        },
        cyan: {
          DEFAULT: '#00f2fe',
          soft: '#e8feff',
          deep: 'rgb(var(--color-accent-text-rgb) / <alpha-value>)',
        },
        aqua: {
          DEFAULT: '#4facfe',
        },
        forest: {
          DEFAULT: '#1e2b26',
          50: '#f4fbf7',
          100: '#e8f4ee',
          700: '#263a32',
          800: '#1b2a24',
          900: '#14201c',
          950: '#101a16',
        },
        /* Legacy alias kept for the fixed-sage copy used on dark brand panels. */
        sage: {
          DEFAULT: 'rgb(var(--color-border-rgb) / <alpha-value>)',
          light: '#dcebe3',
        },
        charcoal: {
          DEFAULT: 'rgb(var(--color-text-rgb) / <alpha-value>)',
          light: 'rgb(var(--color-text-muted-rgb) / <alpha-value>)',
          strong: 'rgb(var(--color-text-strong-rgb) / <alpha-value>)',
        },
        primary: {
          50: 'var(--color-primary-50)',
          500: 'var(--color-primary-500)',
          600: 'var(--color-primary-600)',
          700: 'var(--color-primary-700)',
        },
        surface: 'rgb(var(--color-surface-rgb) / <alpha-value>)',
        'surface-muted': 'rgb(var(--color-surface-muted-rgb) / <alpha-value>)',
        'surface-elevated': 'rgb(var(--color-surface-elevated-rgb) / <alpha-value>)',
        bg: 'rgb(var(--color-bg-rgb) / <alpha-value>)',
        border: 'rgb(var(--color-border-rgb) / <alpha-value>)',
        line: 'rgb(var(--color-border-rgb) / <alpha-value>)',
        sidebar: {
          DEFAULT: 'rgb(var(--color-sidebar-rgb) / <alpha-value>)',
          muted: 'rgb(var(--color-sidebar-muted-rgb) / <alpha-value>)',
          fg: 'rgb(var(--color-sidebar-fg-rgb) / <alpha-value>)',
        },
        'on-accent': '#062a2e',
        success: 'rgb(var(--color-success-rgb) / <alpha-value>)',
        warning: 'rgb(var(--color-warning-rgb) / <alpha-value>)',
        danger: 'rgb(var(--color-danger-rgb) / <alpha-value>)',
        info: 'rgb(var(--color-info-rgb) / <alpha-value>)',
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
      keyframes: {
        'pulse-dot': {
          '0%, 100%': { opacity: '1', transform: 'scale(1)' },
          '50%': { opacity: '0.45', transform: 'scale(0.82)' },
        },
        'bar-rise': {
          from: { transform: 'scaleY(0.18)', opacity: '0.35' },
          to: { transform: 'scaleY(1)', opacity: '1' },
        },
        'float-slow': {
          '0%, 100%': { transform: 'translateY(0)' },
          '50%': { transform: 'translateY(-10px)' },
        },
      },
      animation: {
        'pulse-dot': 'pulse-dot 2.2s ease-in-out infinite',
        'bar-rise': 'bar-rise 700ms cubic-bezier(0.22, 1, 0.36, 1) both',
        'float-slow': 'float-slow 9s ease-in-out infinite',
      },
    },
  },
  plugins: [],
};

export default config;
