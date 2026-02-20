import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useUser } from '../context';
import { AuthPageLayout, AuthCard } from '../components/auth';
import { Button, FormField, AlertBanner } from '../components/ui';

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

    const footer = (
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
    );

    return (
        <AuthPageLayout>
            <AuthCard title="Welcome back" subtitle="Sign in to your ITrade account" footer={footer}>
                <form className="flex flex-col gap-5" onSubmit={handleSubmit} noValidate>
                    {authError && <AlertBanner variant="error">{authError}</AlertBanner>}

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

                    <Button
                        type="submit"
                        variant="primary"
                        fullWidth
                        disabled={isLoading || !email || !password}
                    >
                        {isLoading ? 'Signing in…' : 'Sign in'}
                    </Button>
                </form>
            </AuthCard>
        </AuthPageLayout>
    );
}
