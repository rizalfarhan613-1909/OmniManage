import { redirect } from 'next/navigation';

/** The app shell resolves the role-appropriate workspace; Phase 4A starts at BI. */
export default function RootPage() {
  redirect('/dashboard');
}
