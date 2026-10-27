import {
  Activity,
  AlertTriangle,
  CheckCircle2,
  Circle,
  XCircle,
  type LucideIcon,
} from 'lucide-react';

import { cn } from '@/lib/utils/cn';

export type StatusTone = 'success' | 'info' | 'warning' | 'danger' | 'neutral';

type Props = {
  label: string;
  tone: StatusTone;
  className?: string;
};

const TONE_CLASSES: Record<StatusTone, string> = {
  success:
    'border-mint-accent bg-mint-accent text-on-accent dark:bg-mint-accent/15 dark:text-mint-accent',
  info: 'border-teal-primary/50 bg-teal-primary/15 text-charcoal-strong',
  warning: 'border-warning/40 bg-warning/10 text-warning',
  danger: 'border-danger/40 bg-danger/10 text-danger',
  neutral: 'border-line bg-surface-muted text-charcoal',
};

const TONE_ICONS: Record<StatusTone, LucideIcon> = {
  success: CheckCircle2,
  info: Activity,
  warning: AlertTriangle,
  danger: XCircle,
  neutral: Circle,
};

/** Semantic badge; always pairs an icon with text so colour is never the only signal. */
export function StatusBadge({ label, tone, className }: Props) {
  const Icon = TONE_ICONS[tone];

  return (
    <span
      className={cn(
        'inline-flex items-center gap-1 rounded-full border px-2.5 py-0.5 text-xs font-semibold',
        TONE_CLASSES[tone],
        className,
      )}
    >
      <Icon className="h-3.5 w-3.5 shrink-0" aria-hidden="true" />
      {label}
    </span>
  );
}
