import { cn } from '@/lib/utils/cn';

type LogoSize = 'sm' | 'md' | 'lg' | 'xl';

const SIZE_STYLES: Record<LogoSize, { mark: string; text: string }> = {
  sm: { mark: 'h-8 w-8', text: 'text-[15px]' },
  md: { mark: 'h-10 w-10', text: 'text-lg' },
  lg: { mark: 'h-12 w-12', text: 'text-xl' },
  xl: { mark: 'h-16 w-16', text: 'text-2xl' },
};

type LogoMarkProps = {
  className?: string;
  /** Decorative marks inherit their accessible name from the surrounding link or heading. */
  title?: string;
};

/**
 * The Stitch hexagonal emblem: an interconnected geometric "OM" monogram
 * inside a rounded hexagon with a cyan-to-aqua gradient (#00F2FE -> #4FACFE).
 */
export function LogoMark({ className, title }: LogoMarkProps) {
  return (
    <svg
      viewBox="0 0 64 64"
      className={cn('h-10 w-10 shrink-0 drop-shadow-[0_0_14px_rgba(0,242,254,0.28)]', className)}
      role={title ? 'img' : undefined}
      aria-label={title}
      aria-hidden={title ? undefined : true}
    >
      <defs>
        <linearGradient
          id="om-emblem-stroke"
          x1="8"
          y1="5"
          x2="56"
          y2="59"
          gradientUnits="userSpaceOnUse"
        >
          <stop stopColor="#00F2FE" />
          <stop offset="0.52" stopColor="#22E0E8" />
          <stop offset="1" stopColor="#4FACFE" />
        </linearGradient>
        <linearGradient
          id="om-emblem-fill"
          x1="12"
          y1="8"
          x2="52"
          y2="56"
          gradientUnits="userSpaceOnUse"
        >
          <stop stopColor="#00F2FE" stopOpacity="0.2" />
          <stop offset="1" stopColor="#4FACFE" stopOpacity="0.04" />
        </linearGradient>
      </defs>

      <path
        d="M32 3.5 57 18v28L32 60.5 7 46V18L32 3.5Z"
        fill="url(#om-emblem-fill)"
        stroke="url(#om-emblem-stroke)"
        strokeWidth="3.2"
        strokeLinejoin="round"
      />
      <path
        d="M32 11.8 45.8 19.8v16.1L32 44 18.2 35.9V19.8L32 11.8Z"
        fill="none"
        stroke="url(#om-emblem-stroke)"
        strokeWidth="1.4"
        strokeLinejoin="round"
        opacity="0.35"
      />

      {/* Interconnected monogram: the O ring and the M chevrons share one axis. */}
      <path
        d="M20 24.2 32 31.8l12-7.6"
        fill="none"
        stroke="url(#om-emblem-stroke)"
        strokeWidth="4.4"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <path
        d="M22 39.5v8.2L32 41l10 6.7v-8.2"
        fill="none"
        stroke="url(#om-emblem-stroke)"
        strokeWidth="4.4"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <path
        d="M32 31.8v9.2"
        stroke="url(#om-emblem-stroke)"
        strokeWidth="3"
        strokeLinecap="round"
        opacity="0.9"
      />
      <circle cx="32" cy="31.8" r="2.35" fill="#00F2FE" />
    </svg>
  );
}

type LogoProps = {
  size?: LogoSize;
  theme?: 'light' | 'dark';
  showWordmark?: boolean;
  badge?: string;
  subtitle?: string;
  className?: string;
  markClassName?: string;
  wordmarkClassName?: string;
};

/**
 * Full OmniManage lockup. Use `showWordmark={false}` for rail/collapsed
 * navigation; the emblem remains fully legible at 32px.
 */
export function Logo({
  size = 'md',
  theme = 'light',
  showWordmark = true,
  badge,
  subtitle,
  className,
  markClassName,
  wordmarkClassName,
}: LogoProps) {
  const styles = SIZE_STYLES[size];

  return (
    <span className={cn('inline-flex min-w-0 items-center gap-2.5', className)}>
      <LogoMark className={cn(styles.mark, markClassName)} />
      {showWordmark ? (
        <span className={cn('flex min-w-0 flex-col', wordmarkClassName)}>
          <span className="flex flex-wrap items-center gap-2">
            <span
              className={cn(
                'truncate font-semibold tracking-[-0.03em]',
                styles.text,
                theme === 'dark' ? 'text-white' : 'text-charcoal',
              )}
            >
              OmniManage
            </span>
            {badge ? (
              <span
                className={cn(
                  'inline-flex shrink-0 items-center rounded-md border px-1.5 py-0.5 text-[9px] font-bold uppercase tracking-[0.12em]',
                  theme === 'dark'
                    ? 'border-white/15 bg-white/10 text-sage-light'
                    : 'border-line bg-surface-muted text-charcoal',
                )}
              >
                {badge}
              </span>
            ) : null}
          </span>
          {subtitle ? (
            <span
              className={cn(
                'truncate text-[10px] font-medium uppercase tracking-[0.16em]',
                theme === 'dark' ? 'text-sidebar-muted/70' : 'text-charcoal-light',
              )}
            >
              {subtitle}
            </span>
          ) : null}
        </span>
      ) : null}
    </span>
  );
}
