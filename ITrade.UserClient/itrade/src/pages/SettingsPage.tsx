import { useUser } from '../context';
import {
    ChangeUsernameSection,
    ChangePasswordSection,
    DeleteAccountSection,
    LogoutSection,
} from '../components/settings';

export default function SettingsPage() {
    const { currentUser } = useUser();

    if (!currentUser) return null;

    return (
        <div className="min-h-[calc(100vh-3.5rem)] bg-[linear-gradient(135deg,#020817_0%,#0a1628_45%,#0f2154_100%)]">
            <div className="max-w-2xl mx-auto px-6 py-8 flex flex-col gap-6">
                {/* Header */}
                <div className="flex flex-col gap-1 mb-2">
                    <h1 className="text-2xl font-bold text-slate-200 m-0">Settings</h1>
                    <p className="text-sm text-slate-500 m-0">
                        Manage your account preferences and security.
                    </p>
                </div>

                {/* Account */}
                <ChangeUsernameSection
                    currentUsername={currentUser.username}
                    onUsernameChanged={() => {
                        // Context will pick up the new name on next fetch;
                        // a full page reload is the simplest way to refresh it.
                        window.location.reload();
                    }}
                />

                {/* Security */}
                <ChangePasswordSection />

                {/* Session */}
                <LogoutSection />

                {/* Danger zone */}
                <DeleteAccountSection />
            </div>
        </div>
    );
}
