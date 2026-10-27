import type { ReactNode } from 'react';

type Props = {
  children: ReactNode;
};

export default function AuthLayout({ children }: Props) {
  return <div className="min-h-dvh bg-bg">{children}</div>;
}
