import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useUser } from '../../context';
import { AlertBanner } from '../ui';
import SettingsSection from './SettingsSection';

export default function LogoutSection() {
    const navigate = useNavigate();
    const { logout } = useUser();

    const [isLoggingOut, setIsLoggingOut] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleLogout = async () => {
        setIsLoggingOut(true);
        setError(null);

        try {
            await logout();
            navigate('/login', { replace: true });
        } catch {
            setError('Failed to log out. Please try again.');
            setIsLoggingOut(false);
        }
    };

    return (
        <SettingsSection
            title="Session"
            subtitle="Sign out of your account on this device."
        >
            {error && <AlertBanner variant="error">{error}</AlertBanner>}

            <div className="flex justify-end">
                <button
                    onClick={handleLogout}
                    disabled={isLoggingOut}
                    className="px-5 py-2.5 text-sm font-semibold text-slate-300 bg-white/[0.06] border border-white/10 rounded-xl hover:bg-white/10 hover:text-white transition-all cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                >
                    {isLoggingOut ? 'Logging out…' : 'Log Out'}
                </button>
            </div>
        </SettingsSection>
    );
}
