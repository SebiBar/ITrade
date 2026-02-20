import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useUser } from '../context';

const inputCls =
    'w-full px-4 py-3 bg-white/[0.05] border border-white/10 rounded-xl text-slate-200 text-sm outline-none transition-all placeholder:text-slate-700 focus:border-blue-500 focus:bg-blue-500/[0.06] focus:ring-2 focus:ring-blue-500/20';

export default function LoginPage() {
    const navigate = useNavigate();
    const { login, isLoading, authError, clearAuthError } = useUser();

    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        clearAuthError();
        try {
            await login({ email, password });
            navigate('/dashboard');
        } catch {
            // authError is set inside useUser
        }
    };

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
                    Welcome back
                </h1>
                <p className="text-sm text-slate-500 mt-0 mb-8">
                    Sign in to your ITrade account
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
                            autoComplete="current-password"
                            required
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                        />
                    </div>

                    {/* Submit */}
                    <button
                        type="submit"
                        disabled={isLoading || !email || !password}
                        className="w-full mt-1 py-3.5 px-6 bg-gradient-to-br from-[#3b5bdb] to-[#4f7dff] border-0 rounded-xl text-white text-[0.95rem] font-semibold cursor-pointer transition-all shadow-[0_4px_20px_rgba(79,125,255,0.35)] hover:enabled:opacity-90 hover:enabled:-translate-y-px active:enabled:translate-y-0 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        {isLoading ? 'Signing in…' : 'Sign in'}
                    </button>
                </form>

                {/* Footer */}
                <p className="mt-7 text-center text-sm text-slate-600 mb-0">
                    Don't have an account?{' '}
                    <Link
                        to="/register"
                        onClick={clearAuthError}
                        className="text-blue-400 font-medium hover:text-blue-300 transition-colors no-underline"
                    >
                        Create one
                    </Link>
                </p>
            </div>
        </div>
    );
}
