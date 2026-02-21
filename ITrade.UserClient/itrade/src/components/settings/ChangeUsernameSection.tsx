import { useState, type FormEvent } from 'react';
import { userService } from '../../api/user';
import { FormField, Button, AlertBanner } from '../ui';
import SettingsSection from './SettingsSection';

interface ChangeUsernameSectionProps {
    currentUsername: string;
    onUsernameChanged: (newUsername: string) => void;
}

export default function ChangeUsernameSection({
    currentUsername,
    onUsernameChanged,
}: ChangeUsernameSectionProps) {
    const [username, setUsername] = useState(currentUsername);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);

    const hasChanged = username.trim() !== '' && username.trim() !== currentUsername;

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        if (!hasChanged) return;

        setIsSubmitting(true);
        setError(null);
        setSuccess(null);

        try {
            await userService.changeUsername(username.trim());
            setSuccess('Username updated successfully.');
            onUsernameChanged(username.trim());
        } catch {
            setError('Failed to change username. It may already be taken.');
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <SettingsSection
            title="Change Username"
            subtitle="Update your display name visible to other users."
        >
            <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                {error && <AlertBanner variant="error">{error}</AlertBanner>}
                {success && <AlertBanner variant="success">{success}</AlertBanner>}

                <FormField
                    id="settings-username"
                    label="New Username"
                    value={username}
                    onChange={(e) => {
                        setUsername(e.target.value);
                        setSuccess(null);
                        setError(null);
                    }}
                    placeholder="Enter new username"
                    autoComplete="username"
                />

                <div className="flex justify-end">
                    <Button
                        type="submit"
                        disabled={!hasChanged || isSubmitting}
                    >
                        {isSubmitting ? 'Saving…' : 'Save Username'}
                    </Button>
                </div>
            </form>
        </SettingsSection>
    );
}
