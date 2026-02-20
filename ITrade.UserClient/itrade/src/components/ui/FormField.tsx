import type { InputHTMLAttributes } from 'react';

interface FormFieldProps extends InputHTMLAttributes<HTMLInputElement> {
    label: string;
    id: string;
}

export default function FormField({ label, id, className = '', ...props }: FormFieldProps) {
    return (
        <div className="flex flex-col gap-1.5">
            <label
                htmlFor={id}
                className="text-[0.7rem] font-semibold text-slate-400 uppercase tracking-wider"
            >
                {label}
            </label>
            <input
                id={id}
                className={`w-full px-4 py-3 bg-white/[0.05] border border-white/10 rounded-xl text-slate-200 text-sm outline-none transition-all placeholder:text-slate-700 focus:border-blue-500 focus:bg-blue-500/[0.06] focus:ring-2 focus:ring-blue-500/20 ${className}`}
                {...props}
            />
        </div>
    );
}
