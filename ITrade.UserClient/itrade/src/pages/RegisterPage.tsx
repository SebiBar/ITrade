import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { useUser } from '../context';
import { UserRole } from '../types/enums';

const inputCls =
    'w-full px-4 py-3 bg-white/[0.05] border border-white/10 rounded-xl text-slate-200 text-sm outline-none transition-all placeholder:text-slate-700 focus:border-blue-500 focus:bg-blue-500/[0.06] focus:ring-2 focus:ring-blue-500/20';

export default function RegisterPage() {
    const { register, isLoading, authError, clearAuthError } = useUser();

    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [role, setRole] = useState<UserRole>(UserRole.Client);
    const [success, setSuccess] = useState(false);

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        clearAuthError();
        try {
            await register({ username, email, password, role });
            setSuccess(true);
        } catch {
            // authError is set inside useUser
        }
    };

    if (success) {
        return (
            <div className="min-h-screen flex flex-col items-center justify-center px-4 py-8 bg-[linear-gradient(135deg,#020817_0%,#0a1628_45%,#0f2154_100%)]">
                <div className="flex flex-col items-center gap-1.5 mb-10">
                    <span className="text-[2.2rem] font-extrabold tracking-tight bg-gradient-to-br from-blue-400 via-indigo-400 to-violet-400 bg-clip-text text-transparent">
                        ITrade
                    </span>
                    <span className="text-[0.7rem] text-slate-500 uppercase tracking-[0.15em] font-medium">
                        IT Project Marketplace
                    </span>
                </div>
                <div className="w-full max-w-[440px] bg-white/[0.04] border border-white/[0.08] rounded-2xl p-10 backdrop-blur-xl shadow-[0_0_0_1px_rgba(99,102,241,0.08),0_24px_64px_rgba(0,0,0,0.6),inset_0_1px_0_rgba(255,255,255,0.06)]">
                    <h1 className="text-2xl font-bold text-slate-200 tracking-tight m-0 mb-1.5">Check your inbox</h1>
                    <p className="text-sm text-slate-500 mt-0 mb-6">
                        We've sent a verification link to{' '}
                        <strong className="text-blue-300 font-semibold">{email}</strong>. Click the
                        link to activate your account.
                    </p>
                    <div className="flex items-start gap-2 px-4 py-3 bg-green-500/10 border border-green-500/25 rounded-xl text-green-300 text-sm leading-relaxed" role="status">
                        <svg className="shrink-0 mt-px" width="16" height="16" viewBox="0 0 16 16" fill="none">
                            <circle cx="8" cy="8" r="7" stroke="currentColor" strokeWidth="1.5" />
                            <path d="M5 8l2 2 4-4" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
                        </svg>
                        Account created successfully!
                    </div>
                    <p className="mt-6 text-center text-sm text-slate-600 mb-0">
                        <Link to="/login" onClick={clearAuthError} className="text-blue-400 font-medium hover:text-blue-300 transition-colors no-underline">
                            Back to sign in
                        </Link>
                    </p>
                </div>
            </div>
        );
    }

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

            {/* Card */}
            <div className="w-full max-w-[440px] bg-white/[0.04] border border-white/[0.08] rounded-2xl p-10 backdrop-blur-xl shadow-[0_0_0_1px_rgba(99,102,241,0.08),0_24px_64px_rgba(0,0,0,0.6),inset_0_1px_0_rgba(255,255,255,0.06)]">
                <h1 className="text-2xl font-bold text-slate-200 tracking-tight m-0 mb-1.5">
                    Create an account
                </h1>
                <p className="text-sm text-slate-500 mt-0 mb-8">
                    Join ITrade to post or discover IT projects
                </p>

                <form className="flex flex-col gap-5" onSubmit={handleSubmit} noValidate>

                    {/* Error */}
                    {authError && (
                        <div
                            className="flex items-start gap-2 px-4 py-3 bg-red-500/10 border border-red-500/25 rounded-xl text-red-300 text-sm leading-relaxed"
                            role="alert"
                        >
                            <svg className="shrink-0 mt-px" width="16" height="16" viewBox="0 0 16 16" fill="none">
                                <circle cx="8" cy="8" r="7" stroke="currentColor" strokeWidth="1.5" />
                                <path d="M8 5v3M8 10.5v.5" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" />
                            </svg>
                            {authError}
                        </div>
                    )}

                    {/* Username */}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-[0.7rem] font-semibold text-slate-400 uppercase tracking-wider" htmlFor="username">
                            Username
                        </label>
                        <input
                            id="username"
                            className={inputCls}
                            type="text"
                            placeholder="johndoe"
                            autoComplete="username"
                            required
                            value={username}
                            onChange={(e) => setUsername(e.target.value)}
                        />
                    </div>

                    {/* Email */}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-[0.7rem] font-semibold text-slate-400 uppercase tracking-wider" htmlFor="email">
                            Email
                        </label>
                        <input
                            id="email"
                            className={inputCls}
                            type="email"
                            placeholder="you@example.com"
                            autoComplete="email"
                            required
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                        />
                    </div>

                    {/* Password */}
                    <div className="flex flex-col gap-1.5">
                        <label className="text-[0.7rem] font-semibold text-slate-400 uppercase tracking-wider" htmlFor="password">
                            Password
                        </label>
                        <input
                            id="password"
                            className={inputCls}
                            type="password"
                            placeholder="••••••••"
                            autoComplete="new-password"
                            required
                            minLength={8}
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                        />
                    </div>

                    {/* Role */}
                    <div className="flex flex-col gap-1.5">
                        <span className="text-[0.7rem] font-semibold text-slate-400 uppercase tracking-wider">
                            I am a…
                        </span>
                        <div className="grid grid-cols-2 gap-2">
                            <button
                                type="button"
                                onClick={() => setRole(UserRole.Client)}
                                className={`flex items-center justify-center gap-1.5 py-2.5 px-4 rounded-lg border text-sm font-medium transition-all cursor-pointer ${role === UserRole.Client
                                        ? 'bg-blue-500/15 border-blue-500 text-blue-300'
                                        : 'bg-white/[0.04] border-white/10 text-slate-500 hover:border-blue-500/40 hover:text-slate-400'
                                    }`}
                            >
                                <svg width="15" height="15" viewBox="0 0 24 24" fill="none">
                                    <rect x="3" y="7" width="18" height="13" rx="2" stroke="currentColor" strokeWidth="1.75" />
                                    <path d="M16 7V6a4 4 0 10-8 0v1" stroke="currentColor" strokeWidth="1.75" />
                                </svg>
                                Client
                            </button>
                            <button
                                type="button"
                                onClick={() => setRole(UserRole.Specialist)}
                                className={`flex items-center justify-center gap-1.5 py-2.5 px-4 rounded-lg border text-sm font-medium transition-all cursor-pointer ${role === UserRole.Specialist
                                        ? 'bg-blue-500/15 border-blue-500 text-blue-300'
                                        : 'bg-white/[0.04] border-white/10 text-slate-500 hover:border-blue-500/40 hover:text-slate-400'
                                    }`}
                            >
                                <svg width="15" height="15" viewBox="0 0 24 24" fill="none">
                                    <path d="M12 2L2 7l10 5 10-5-10-5z" stroke="currentColor" strokeWidth="1.75" strokeLinejoin="round" />
                                    <path d="M2 17l10 5 10-5M2 12l10 5 10-5" stroke="currentColor" strokeWidth="1.75" strokeLinejoin="round" />
                                </svg>
                                Specialist
                            </button>
                        </div>
                    </div>

                    {/* Submit */}
                    <button
                        type="submit"
                        disabled={isLoading || !username || !email || !password}
                        className="w-full mt-1 py-3.5 px-6 bg-gradient-to-br from-[#3b5bdb] to-[#4f7dff] border-0 rounded-xl text-white text-[0.95rem] font-semibold cursor-pointer transition-all shadow-[0_4px_20px_rgba(79,125,255,0.35)] hover:enabled:opacity-90 hover:enabled:-translate-y-px active:enabled:translate-y-0 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        {isLoading ? 'Creating account…' : 'Create account'}
                    </button>
                </form>

                {/* Footer */}
                <p className="mt-7 text-center text-sm text-slate-600 mb-0">
                    Already have an account?{' '}
                    <Link
                        to="/login"
                        onClick={clearAuthError}
                        className="text-blue-400 font-medium hover:text-blue-300 transition-colors no-underline"
                    >
                        Sign in
                    </Link>
                </p>
            </div>
        </div>
    );
}
