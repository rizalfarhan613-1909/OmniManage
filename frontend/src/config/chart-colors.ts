/**
 * Recharts needs concrete SVG colour values (it cannot resolve Tailwind
 * classes). These CSS variables are declared in `src/styles/tokens.css` and
 * swapped by the `.dark` class, so charts follow the active theme without
 * re-rendering their palette in JavaScript.
 */
export const CHART_COLORS = {
  revenue: 'var(--chart-revenue)',
  cashFlow: 'var(--chart-cash-flow)',
  grid: 'var(--chart-grid)',
  axis: 'var(--chart-axis)',
  surface: 'var(--chart-surface)',
  charcoal: 'var(--chart-text)',
} as const;
