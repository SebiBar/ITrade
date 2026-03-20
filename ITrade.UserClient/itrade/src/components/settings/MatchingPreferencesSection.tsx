import { useState, type FormEvent } from 'react';
import { userService } from '../../api/user';
import { Button, AlertBanner } from '../ui';
import SettingsSection from './SettingsSection';
import { MatchingPreferencesEnum } from '../../types/enums';

export default function MatchingPreferencesSection() {
    const [preference, setPreference] = useState<MatchingPreferencesEnum | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        if (preference === null) return;

        setIsSubmitting(true);
        setError(null);
        setSuccess(null);

        try {
            await userService.updateMatchingPreferences(preference);
            setSuccess('Matching preferences updated successfully.');
        } catch {
            setError('Failed to update matching preferences.');
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <SettingsSection
            title="Matching Preferences"
            subtitle="Choose what aspects you value most when finding matches."
        >
            <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                {error && <AlertBanner variant="error">{error}</AlertBanner>}
                {success && <AlertBanner variant="success">{success}</AlertBanner>}

                <div className="flex flex-col gap-3">
                    <label className="flex items-start gap-3 cursor-pointer">
                        <input
                            type="radio"
                            name="matchingPreference"
                            value={MatchingPreferencesEnum.Balanced}
                            checked={preference === MatchingPreferencesEnum.Balanced}
                            onChange={() => setPreference(MatchingPreferencesEnum.Balanced)}
                            className="mt-1"
                        />
                        <div className="flex flex-col">
                            <span className="text-slate-200 font-medium">Balanced</span>
                            <span className="text-sm text-slate-400">
                                A mix of skill tags, experience, and reputation.
                            </span>
                        </div>
                    </label>

                    <label className="flex items-start gap-3 cursor-pointer">
                        <input
                            type="radio"
                            name="matchingPreference"
                            value={MatchingPreferencesEnum.PrioritizeSkills}
                            checked={preference === MatchingPreferencesEnum.PrioritizeSkills}
                            onChange={() => setPreference(MatchingPreferencesEnum.PrioritizeSkills)}
                            className="mt-1"
                        />
                        <div className="flex flex-col">
                            <span className="text-slate-200 font-medium">Prioritize Skills</span>
                            <span className="text-sm text-slate-400">
                                Focus heavily on exact tag matches.
                            </span>
                        </div>
                    </label>

                    <label className="flex items-start gap-3 cursor-pointer">
                        <input
                            type="radio"
                            name="matchingPreference"
                            value={MatchingPreferencesEnum.PrioritizeReputation}
                            checked={preference === MatchingPreferencesEnum.PrioritizeReputation}
                            onChange={() => setPreference(MatchingPreferencesEnum.PrioritizeReputation)}
                            className="mt-1"
                        />
                        <div className="flex flex-col">
                            <span className="text-slate-200 font-medium">Prioritize Reputation</span>
                            <span className="text-sm text-slate-400">
                                Value reviews and completed projects over exact skill matching.
                            </span>
                        </div>
                    </label>
                </div>

                <div className="flex justify-end mt-2">
                    <Button type="submit" disabled={isSubmitting || preference === null}>
                        {isSubmitting ? 'Saving...' : 'Save Preferences'}
                    </Button>
                </div>
            </form>
        </SettingsSection>
    );
}
