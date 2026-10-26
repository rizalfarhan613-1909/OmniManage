import type { ReactNode } from 'react';

type Props = {
  children: ReactNode;
};

export default function AuthLayout({ children }: Props) {
  return (
    <div className="flex min-h-dvh items-center justify-center bg-bg p-4 sm:p-6">{children}</div>
  );
}
