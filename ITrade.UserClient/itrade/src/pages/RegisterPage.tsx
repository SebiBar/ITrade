import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useUser } from '../context';
import { UserRole } from '../types/enums';
import { AuthPageLayout, AuthCard, RoleToggle } from '../components/auth';
import { Button, FormField, AlertBanner } from '../components/ui';

export default function RegisterPage() {
    const navigate = useNavigate();
    const { register, isLoading, authError, clearAuthError } = useUser();

    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [role, setRole] = useState<UserRole>(UserRole.Client);

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        clearAuthError();
        try {
            await register({ username, email, password, role });
            window.alert('Check your email to validate');
            navigate('/login');
        } catch {
            // authError is set inside useUser
        }
    };

    const footer = (
        <>
            Already have an account?{' '}
            <Link
                to="/login"
                onClick={clearAuthError}
                className="text-blue-400 font-medium hover:text-blue-300 transition-colors no-underline"
            >
                Sign in
            </Link>
        </>
    );

    return (
        <AuthPageLayout>
            <AuthCard title="Create an account" subtitle="Join ITrade to post or discover IT projects" footer={footer}>
                <form className="flex flex-col gap-5" onSubmit={handleSubmit} noValidate>
                    {authError && <AlertBanner variant="error">{authError}</AlertBanner>}

                    <FormField
                        label="Username"
                        id="username"
                        type="text"
                        placeholder="johndoe"
                        autoComplete="username"
                        required
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                    />

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
                        autoComplete="new-password"
                        required
                        minLength={8}
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                    />

                    <RoleToggle value={role} onChange={setRole} />

                    <Button
                        type="submit"
                        variant="primary"
                        fullWidth
                        disabled={isLoading || !username || !email || !password}
                    >
                        {isLoading ? 'Creating account…' : 'Create account'}
                    </Button>
                </form>
            </AuthCard>
        </AuthPageLayout>
    );
}
