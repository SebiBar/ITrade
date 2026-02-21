import type { ReactNode } from 'react';

interface SettingsSectionProps {
    title: string;
    subtitle?: string;
    children: ReactNode;
    /** Optional red/danger border styling */
    danger?: boolean;
}

/** Reusable card wrapper for each settings block */
export default function SettingsSection({
    title,
    subtitle,
    children,
    danger = false,
}: SettingsSectionProps) {
    return (
        <section
            className={`flex flex-col gap-5 p-6 rounded-2xl border ${danger
                    ? 'bg-red-500/[0.03] border-red-500/15'
                    : 'bg-white/[0.03] border-white/[0.06]'
                }`}
        >
            <div className="flex flex-col gap-1">
                <h2
                    className={`text-base font-semibold m-0 ${danger ? 'text-red-300' : 'text-slate-200'
                        }`}
                >
                    {title}
                </h2>
                {subtitle && (
                    <p className="text-sm text-slate-500 m-0">{subtitle}</p>
                )}
            </div>
            {children}
        </section>
    );
}
