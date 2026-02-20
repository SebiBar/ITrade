import type { UserRole } from '../../types/enums';
import { UserRole as Role } from '../../types/enums';

interface RoleToggleProps {
    value: UserRole;
    onChange: (role: UserRole) => void;
}

const options: { label: string; value: UserRole; icon: React.ReactNode }[] = [
    {
        label: 'Client',
        value: Role.Client,
        icon: (
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none">
                <rect x="3" y="7" width="18" height="13" rx="2" stroke="currentColor" strokeWidth="1.75" />
                <path d="M16 7V6a4 4 0 10-8 0v1" stroke="currentColor" strokeWidth="1.75" />
            </svg>
        ),
    },
    {
        label: 'Specialist',
        value: Role.Specialist,
        icon: (
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none">
                <path d="M12 2L2 7l10 5 10-5-10-5z" stroke="currentColor" strokeWidth="1.75" strokeLinejoin="round" />
                <path d="M2 17l10 5 10-5M2 12l10 5 10-5" stroke="currentColor" strokeWidth="1.75" strokeLinejoin="round" />
            </svg>
        ),
    },
];

/**
 * Two-button role selector (Client / Specialist).
 * Reusable in registration and profile-edit flows.
 */
export default function RoleToggle({ value, onChange }: RoleToggleProps) {
    return (
        <div className="flex flex-col gap-1.5">
            <span className="text-[0.7rem] font-semibold text-slate-400 uppercase tracking-wider">
                I am a…
            </span>
            <div className="grid grid-cols-2 gap-2">
                {options.map((opt) => (
                    <button
                        key={opt.value}
                        type="button"
                        onClick={() => onChange(opt.value)}
                        className={`flex items-center justify-center gap-1.5 py-2.5 px-4 rounded-lg border text-sm font-medium transition-all cursor-pointer ${value === opt.value
                                ? 'bg-blue-500/15 border-blue-500 text-blue-300'
                                : 'bg-white/[0.04] border-white/10 text-slate-500 hover:border-blue-500/40 hover:text-slate-400'
                            }`}
                    >
                        {opt.icon}
                        {opt.label}
                    </button>
                ))}
            </div>
        </div>
    );
}
