/**
 * Recharts needs concrete SVG colour values (it cannot resolve Tailwind
 * classes). This is the single permitted mirror of the DESIGN.md palette for
 * chart internals; every other component uses Tailwind tokens.
 */
export const CHART_COLORS = {
  revenue: '#14D6E4',
  cashFlow: '#8AFFC9',
  grid: '#C9DAD3',
  axis: '#6B7C75',
  surface: '#FFFFFF',
  charcoal: '#4A3A3A',
} as const;
