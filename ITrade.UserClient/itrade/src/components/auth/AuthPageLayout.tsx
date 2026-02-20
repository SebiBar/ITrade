import type { ReactNode } from 'react';

interface AuthPageLayoutProps {
    children: ReactNode;
}

/**
 * Full-page dark-blue gradient shell with the ITrade brand header.
 * Wrap any auth-related page (login, register, forgot-password, etc.) with this.
 */
export default function AuthPageLayout({ children }: AuthPageLayoutProps) {
    return (
        <div className="min-h-screen flex flex-col items-center justify-center px-4 py-8 bg-[linear-gradient(135deg,#020817_0%,#0a1628_45%,#0f2154_100%)]">
            {/* Brand */}
            <div className="flex flex-col items-center gap-1.5 mb-10">
                <span className="text-[2.2rem] font-extrabold tracking-tight bg-gradient-to-br from-blue-400 via-indigo-400 to-violet-400 bg-clip-text text-transparent">
                    ITrade
                </span>
                <span className="text-[0.7rem] text-slate-500 uppercase tracking-[0.15em] font-medium">
                    IT Project Marketplace
                </span>
            </div>

            {children}
        </div>
    );
}
