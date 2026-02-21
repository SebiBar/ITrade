import { useState, type FormEvent } from 'react';
import { authService } from '../../api/auth';
import { FormField, Button, AlertBanner } from '../ui';
import SettingsSection from './SettingsSection';

export default function ChangePasswordSection() {
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);

    const isValid =
        newPassword.length >= 6 &&
        newPassword === confirmPassword;

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        if (!isValid) return;

        setIsSubmitting(true);
        setError(null);
        setSuccess(null);

        try {
            await authService.changePassword(newPassword);
            setSuccess('Password changed successfully.');
            setNewPassword('');
            setConfirmPassword('');
        } catch {
            setError('Failed to change password. Please try again.');
        } finally {
            setIsSubmitting(false);
        }
    };

    const clearMessages = () => {
        setError(null);
        setSuccess(null);
    };

    return (
        <SettingsSection
            title="Change Password"
            subtitle="Choose a strong password with at least 6 characters."
        >
            <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                {error && <AlertBanner variant="error">{error}</AlertBanner>}
                {success && <AlertBanner variant="success">{success}</AlertBanner>}

                <FormField
                    id="settings-new-password"
                    label="New Password"
                    type="password"
                    value={newPassword}
                    onChange={(e) => {
                        setNewPassword(e.target.value);
                        clearMessages();
                    }}
                    placeholder="Enter new password"
                    autoComplete="new-password"
                />

                <FormField
                    id="settings-confirm-password"
                    label="Confirm Password"
                    type="password"
                    value={confirmPassword}
                    onChange={(e) => {
                        setConfirmPassword(e.target.value);
                        clearMessages();
                    }}
                    placeholder="Re-enter new password"
                    autoComplete="new-password"
                />

                {confirmPassword && confirmPassword !== newPassword && (
                    <p className="text-xs text-red-400 m-0">Passwords do not match.</p>
                )}

                <div className="flex justify-end">
                    <Button
                        type="submit"
                        disabled={!isValid || isSubmitting}
                    >
                        {isSubmitting ? 'Updating…' : 'Update Password'}
                    </Button>
                </div>
            </form>
        </SettingsSection>
    );
}
