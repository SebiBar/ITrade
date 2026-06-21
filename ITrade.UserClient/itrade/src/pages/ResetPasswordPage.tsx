import { useState, useEffect, type FormEvent } from 'react';
import { useSearchParams, useNavigate, Link } from 'react-router-dom';
import { AuthPageLayout, AuthCard } from '../components/auth';
import { Button, FormField, AlertBanner } from '../components/ui';
import { authService } from '../api';

export default function ResetPasswordPage() {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const token = searchParams.get('token');

    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');

    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState(false);

    useEffect(() => {
        if (!token) {
            setError('Invalid or missing reset token.');
        }
    }, [token]);

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        setError('');

        if (!token) {
            setError('Missing token.');
            return;
        }

        if (password !== confirmPassword) {
            setError('Passwords do not match.');
            return;
        }

        if (password.length < 6) {
            setError('Password must be at least 6 characters.');
            return;
        }

        setIsLoading(true);
        try {
            await authService.resolveForgotPassword({
                emailedToken: token,
                newPassword: password
            });
            setSuccess(true);
        } catch (err: any) {
            setError(err?.response?.data?.message || 'Failed to reset password. The link might be expired.');
        } finally {
            setIsLoading(false);
        }
    };

    if (success) {
        return (
            <AuthPageLayout>
                <AuthCard title="Password reset" subtitle="Your password has been successfully reset.">
                    <div className="flex flex-col gap-4">
                        <AlertBanner variant="success">
                            You can now sign in with your new password.
                        </AlertBanner>
                        <Button variant="primary" onClick={() => navigate('/login')} fullWidth>
                            Go to login
                        </Button>
                    </div>
                </AuthCard>
            </AuthPageLayout>
        );
    }

    const footer = (
        <>
            Remembered your password?{' '}
            <Link
                to="/login"
                className="text-blue-400 font-medium hover:text-blue-300 transition-colors no-underline"
            >
                Sign in
            </Link>
        </>
    );

    return (
        <AuthPageLayout>
            <AuthCard title="Reset password" subtitle="Enter your new password below." footer={footer}>
                {!token ? (
                    <AlertBanner variant="error">{error}</AlertBanner>
                ) : (
                    <form className="flex flex-col gap-5" onSubmit={handleSubmit} noValidate>
                        {error && <AlertBanner variant="error">{error}</AlertBanner>}

                        <FormField
                            label="New Password"
                            id="password"
                            type="password"
                            placeholder="••••••••"
                            required
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                        />

                        <FormField
                            label="Confirm New Password"
                            id="confirmPassword"
                            type="password"
                            placeholder="••••••••"
                            required
                            value={confirmPassword}
                            onChange={(e) => setConfirmPassword(e.target.value)}
                        />

                        <Button
                            type="submit"
                            variant="primary"
                            fullWidth
                            disabled={isLoading || !password || !confirmPassword}
                        >
                            {isLoading ? 'Resetting...' : 'Reset password'}
                        </Button>
                    </form>
                )}
            </AuthCard>
        </AuthPageLayout>
    );
}
