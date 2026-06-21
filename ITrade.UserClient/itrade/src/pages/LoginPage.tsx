import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useUser } from '../context';
import { AuthPageLayout, AuthCard } from '../components/auth';
import { Button, FormField, AlertBanner } from '../components/ui';
import { authService } from '../api';

export default function LoginPage() {
    const navigate = useNavigate();
    const { login, isLoading, authError, clearAuthError } = useUser();

    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');

    // Forgot Password State
    const [isForgotMode, setIsForgotMode] = useState(false);
    const [resetSent, setResetSent] = useState(false);
    const [forgotLoading, setForgotLoading] = useState(false);
    const [forgotError, setForgotError] = useState('');

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        clearAuthError();

        if (isForgotMode) {
            setForgotError('');
            setForgotLoading(true);
            try {
                await authService.forgotPassword(email);
                setResetSent(true);
            } catch (err: any) {
                setForgotError(err?.response?.data?.message || 'Failed to send reset link.');
            } finally {
                setForgotLoading(false);
            }
            return;
        }

        try {
            await login({ email, password });
            navigate('/dashboard');
        } catch {
            // authError is set inside useUser
        }
    };

    const toggleMode = () => {
        setIsForgotMode(!isForgotMode);
        clearAuthError();
        setForgotError('');
        setResetSent(false);
    };

    const footer = (
        <>
            {isForgotMode ? (
                <>
                    Remember your password?{' '}
                    <button
                        type="button"
                        onClick={toggleMode}
                        className="text-blue-400 font-medium hover:text-blue-300 transition-colors bg-transparent border-none p-0 cursor-pointer"
                    >
                        Sign in
                    </button>
                </>
            ) : (
                <>
                    Don't have an account?{' '}
                    <Link
                        to="/register"
                        onClick={clearAuthError}
                        className="text-blue-400 font-medium hover:text-blue-300 transition-colors no-underline"
                    >
                        Create one
                    </Link>
                </>
            )}
        </>
    );

    if (resetSent) {
        return (
            <AuthPageLayout>
                <AuthCard title="Check your email" subtitle="We've sent you a password reset link." footer={footer}>
                    <AlertBanner variant="success">
                        If an account exists for {email}, you will receive an email with instructions on how to reset your password.
                    </AlertBanner>
                </AuthCard>
            </AuthPageLayout>
        );
    }

    return (
        <AuthPageLayout>
            <AuthCard
                title={isForgotMode ? "Reset your password" : "Welcome back"}
                subtitle={isForgotMode ? "Enter your email to receive a reset link" : "Sign in to your ITrade account"}
                footer={footer}
            >
                <form className="flex flex-col gap-5" onSubmit={handleSubmit} noValidate>
                    {authError && !isForgotMode && <AlertBanner variant="error">{authError}</AlertBanner>}
                    {forgotError && isForgotMode && <AlertBanner variant="error">{forgotError}</AlertBanner>}

                    <FormField
                        label="Email"
                        id="email"
                        type="email"
                        placeholder="you@example.com"
                        autoComplete="email"
                        required
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                    />

                    {!isForgotMode && (
                        <div className="flex flex-col gap-1">
                            <FormField
                                label="Password"
                                id="password"
                                type="password"
                                placeholder="••••••••"
                                autoComplete="current-password"
                                required
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                            />
                            <div className="flex justify-end">
                                <button
                                    type="button"
                                    onClick={toggleMode}
                                    className="text-xs text-slate-400 hover:text-white transition-colors bg-transparent border-none p-0 cursor-pointer"
                                >
                                    Forgot password?
                                </button>
                            </div>
                        </div>
                    )}

                    <Button
                        type="submit"
                        variant="primary"
                        fullWidth
                        disabled={isForgotMode ? (forgotLoading || !email) : (isLoading || !email || !password)}
                    >
                        {isForgotMode
                            ? (forgotLoading ? 'Sending...' : 'Send reset link')
                            : (isLoading ? 'Signing in…' : 'Sign in')}
                    </Button>
                </form>
            </AuthCard>
        </AuthPageLayout>
    );
}
