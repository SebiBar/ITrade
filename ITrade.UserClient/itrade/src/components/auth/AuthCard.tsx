import type { ReactNode } from 'react';

interface AuthCardProps {
    title: string;
    subtitle?: string;
    children: ReactNode;
    /** Optional link row rendered below the form (e.g. "Already have an account?") */
    footer?: ReactNode;
}

/**
 * Frosted-glass card used on every auth page.
 * Accepts a title, optional subtitle, main content, and an optional footer link row.
 */
export default function AuthCard({ title, subtitle, children, footer }: AuthCardProps) {
    return (
        <div className="w-full max-w-[440px] bg-white/[0.04] border border-white/[0.08] rounded-2xl p-10 backdrop-blur-xl shadow-[0_0_0_1px_rgba(99,102,241,0.08),0_24px_64px_rgba(0,0,0,0.6),inset_0_1px_0_rgba(255,255,255,0.06)]">
            <h1 className="text-2xl font-bold text-slate-200 tracking-tight m-0 mb-1.5">
                {title}
            </h1>
            {subtitle && (
                <p className="text-sm text-slate-500 mt-0 mb-8">{subtitle}</p>
            )}

            {children}

            {footer && (
                <p className="mt-7 text-center text-sm text-slate-600 mb-0">
                    {footer}
                </p>
            )}
        </div>
    );
}
