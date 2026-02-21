import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { userService } from '../../api/user';
import { useUser } from '../../context';
import { Button, AlertBanner } from '../ui';
import SettingsSection from './SettingsSection';

export default function DeleteAccountSection() {
    const navigate = useNavigate();
    const { logout } = useUser();

    const [showConfirm, setShowConfirm] = useState(false);
    const [isDeleting, setIsDeleting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleDelete = async () => {
        setIsDeleting(true);
        setError(null);

        try {
            await userService.softDeleteAccount();
            await logout();
            navigate('/login', { replace: true });
        } catch {
            setError('Failed to delete account. Please try again.');
            setIsDeleting(false);
        }
    };

    return (
        <SettingsSection
            title="Delete Account"
            subtitle="Delete your account and all associated data."
            danger
        >
            {error && <AlertBanner variant="error">{error}</AlertBanner>}

            {!showConfirm ? (
                <div className="flex justify-end">
                    <button
                        onClick={() => setShowConfirm(true)}
                        className="px-5 py-2.5 text-sm font-semibold text-red-400 bg-red-500/10 border border-red-500/20 rounded-xl hover:bg-red-500/20 hover:text-red-300 transition-all cursor-pointer"
                    >
                        Delete My Account
                    </button>
                </div>
            ) : (
                <div className="flex flex-col gap-4 p-4 bg-red-500/[0.06] border border-red-500/20 rounded-xl">
                    <p className="text-sm text-red-300 m-0">
                        Are you sure? This will deactivate your account. You will be logged out immediately.
                    </p>
                    <div className="flex items-center gap-3 justify-end">
                        <Button
                            variant="ghost"
                            onClick={() => setShowConfirm(false)}
                            disabled={isDeleting}
                        >
                            Cancel
                        </Button>
                        <button
                            onClick={handleDelete}
                            disabled={isDeleting}
                            className="px-5 py-2.5 text-sm font-semibold text-white bg-red-600 border-none rounded-xl hover:bg-red-500 transition-all cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            {isDeleting ? 'Deleting…' : 'Yes, Delete My Account'}
                        </button>
                    </div>
                </div>
            )}
        </SettingsSection>
    );
}
