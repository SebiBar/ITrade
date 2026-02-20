import type { ButtonHTMLAttributes, ReactNode } from 'react';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    /** Visual variant */
    variant?: 'primary' | 'ghost';
    children: ReactNode;
    /** Full-width block button */
    fullWidth?: boolean;
}

export default function Button({
    variant = 'primary',
    fullWidth = false,
    className = '',
    children,
    ...props
}: ButtonProps) {
    const base =
        'inline-flex items-center justify-center gap-2 rounded-xl font-semibold text-sm transition-all cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed';

    const variants = {
        primary:
            'py-3.5 px-6 bg-gradient-to-br from-[#3b5bdb] to-[#4f7dff] border-0 text-white text-[0.95rem] shadow-[0_4px_20px_rgba(79,125,255,0.35)] hover:enabled:opacity-90 hover:enabled:-translate-y-px active:enabled:translate-y-0',
        ghost:
            'py-2.5 px-6 bg-white/[0.06] border border-white/10 text-slate-400 hover:enabled:bg-white/10 hover:enabled:text-slate-300',
    };

    return (
        <button
            className={`${base} ${variants[variant]} ${fullWidth ? 'w-full' : ''} ${className}`}
            {...props}
        >
            {children}
        </button>
    );
}
