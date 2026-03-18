import { useEffect, useRef, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { authService } from '../api/auth';
import { AuthPageLayout, AuthCard } from '../components/auth';
import { AlertBanner } from '../components/ui';

type VerificationState = 'verifying' | 'success' | 'error';

export default function VerifyEmailPage() {
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const [state, setState] = useState<VerificationState>('verifying');
    const [error, setError] = useState<string | null>(null);
    const token = searchParams.get('token');
    // We use a ref to prevent double-calls in strict mode, 
    // but without using a sticky global variable that breaks HMR.
    const hasAttempted = useRef(false);

    useEffect(() => {
        if (!token) {
            setState('error');
            setError('HTTP 400: Missing email verification token.');
            return;
        }

        if (hasAttempted.current) {
            return;
        }
        hasAttempted.current = true;

        setState('verifying');

        authService.verifyEmail(token)
            .then(() => {
                setState('success');
                // Short timeout to let the user see the success message
                setTimeout(() => {
                    navigate('/login', { replace: true });
                }, 1500);
            })
            .catch((err: unknown) => {
                const message =
                    err instanceof Error
                        ? err.message
                        : 'Email verification failed. Please request a new verification email.';
                setError(message);
                setState('error');
            });

    }, [token, navigate]);

    const footer = (
        <Link
            to="/login"
            className="text-blue-400 font-medium hover:text-blue-300 transition-colors no-underline"
        >
            Back to sign in
        </Link>
    );

    return (
        <AuthPageLayout>
            <AuthCard
                title="Verify your email"
                subtitle="We are validating your email confirmation link."
                footer={footer}
            >
                {state === 'verifying' && (
                    <AlertBanner variant="success">
                        Validating your email address...
                    </AlertBanner>
                )}

                {state === 'success' && (
                    <AlertBanner variant="success">
                        Email verified successfully. Redirecting to sign in...
                    </AlertBanner>
                )}

                {state === 'error' && (
                    <AlertBanner variant="error">
                        {error ?? 'Email verification failed. Please request a new verification email.'}
                    </AlertBanner>
                )}
            </AuthCard>
        </AuthPageLayout>
    );
}