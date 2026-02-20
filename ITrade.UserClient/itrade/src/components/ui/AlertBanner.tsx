import type { ReactNode } from 'react';

type AlertVariant = 'error' | 'success';

interface AlertBannerProps {
    variant: AlertVariant;
    children: ReactNode;
}

const icons: Record<AlertVariant, ReactNode> = {
    error: (
        <svg className="shrink-0 mt-px" width="16" height="16" viewBox="0 0 16 16" fill="none">
            <circle cx="8" cy="8" r="7" stroke="currentColor" strokeWidth="1.5" />
            <path d="M8 5v3M8 10.5v.5" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
        </svg>
    ),
    success: (
        <svg className="shrink-0 mt-px" width="16" height="16" viewBox="0 0 16 16" fill="none">
            <circle cx="8" cy="8" r="7" stroke="currentColor" strokeWidth="1.5" />
            <path d="M5 8l2 2 4-4" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
    ),
};

const styles: Record<AlertVariant, string> = {
    error: 'bg-red-500/10 border-red-500/25 text-red-300',
    success: 'bg-green-500/10 border-green-500/25 text-green-300',
};

const roles: Record<AlertVariant, string> = {
    error: 'alert',
    success: 'status',
};

export default function AlertBanner({ variant, children }: AlertBannerProps) {
    return (
        <div
            className={`flex items-start gap-2 px-4 py-3 border rounded-xl text-sm leading-relaxed ${styles[variant]}`}
            role={roles[variant]}
        >
            {icons[variant]}
            {children}
        </div>
    );
}
