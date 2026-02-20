import type { ReactNode } from 'react';

interface DashboardSectionProps {
    title: string;
    /** Optional badge count shown next to title */
    badge?: number;
    /** Optional subtitle description */
    subtitle?: string;
    children: ReactNode;
    className?: string;
}

/** Reusable section wrapper for dashboard content blocks */
export default function DashboardSection({
    title,
    badge,
    subtitle,
    children,
    className = '',
}: DashboardSectionProps) {
    return (
        <section className={`flex flex-col gap-4 ${className}`}>
            <div className="flex items-center gap-3">
                <h2 className="text-lg font-semibold text-slate-200 m-0">{title}</h2>
                {badge !== undefined && badge > 0 && (
                    <span className="min-w-[1.5rem] h-6 px-2 bg-blue-500/20 text-blue-400 text-xs font-bold rounded-full flex items-center justify-center">
                        {badge}
                    </span>
                )}
            </div>
            {subtitle && (
                <p className="text-sm text-slate-500 -mt-2 m-0">{subtitle}</p>
            )}
            {children}
        </section>
    );
}
