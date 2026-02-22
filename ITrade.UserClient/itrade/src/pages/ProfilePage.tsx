import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { userService } from '../api';
import { useUser } from '../context';
import type { UserProfileResponse } from '../types';
import {
    ProfileHeader,
    ProfileDetails,
    ProfileTabs,
    ProfileProjectCard,
    ProfileReviewCard,
} from '../components/profile';
import type { ProfileTab } from '../components/profile';

export default function ProfilePage() {
    const { userId } = useParams<{ userId: string }>();
    const navigate = useNavigate();
    const { currentUser } = useUser();

    const [profile, setProfile] = useState<UserProfileResponse | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [activeTab, setActiveTab] = useState<ProfileTab>('projects');
    const [isHardDeleting, setIsHardDeleting] = useState(false);
    const [actionError, setActionError] = useState<string | null>(null);

    const isOwnProfile = currentUser?.id === Number(userId);
    const isAdmin = currentUser?.role === 'Admin';

    const fetchProfile = useCallback(async () => {
        if (!userId) return;
        setIsLoading(true);
        setError(null);
        try {
            const data = await userService.getUserProfile(Number(userId));
            setProfile(data);
        } catch {
            setError('Failed to load user profile.');
        } finally {
            setIsLoading(false);
        }
    }, [userId]);

    useEffect(() => {
        fetchProfile();
    }, [fetchProfile]);

    const handleHardDeleteUser = async () => {
        if (!userId) return;
        const confirmed = window.confirm(
            `PERMANENTLY delete user "${profile?.user.username}"? This cannot be undone.`
        );
        if (!confirmed) return;
        setIsHardDeleting(true);
        setActionError(null);
        try {
            await userService.hardDeleteUser(Number(userId));
            navigate(-1);
        } catch {
            setActionError('Failed to permanently delete user.');
        } finally {
            setIsHardDeleting(false);
        }
    };

    // ── Loading / Error ───────────────────────────────────────────────────────

    if (isLoading) {
        return (
            <div className="min-h-[calc(100vh-3.5rem)] bg-[linear-gradient(135deg,#020817_0%,#0a1628_45%,#0f2154_100%)] flex items-center justify-center">
                <div className="flex flex-col items-center gap-3">
                    <div className="w-8 h-8 border-2 border-blue-500/30 border-t-blue-400 rounded-full animate-spin" />
                    <span className="text-sm text-slate-500">Loading profile…</span>
                </div>
            </div>
        );
    }

    if (error || !profile) {
        return (
            <div className="min-h-[calc(100vh-3.5rem)] bg-[linear-gradient(135deg,#020817_0%,#0a1628_45%,#0f2154_100%)] flex items-center justify-center">
                <div className="flex flex-col items-center gap-3 text-center">
                    <p className="text-sm text-red-400 m-0">{error ?? 'User not found.'}</p>
                    <button
                        onClick={() => navigate(-1)}
                        className="px-4 py-2 text-xs font-semibold text-blue-400 bg-blue-500/10 border border-blue-500/20 rounded-lg hover:bg-blue-500/20 transition-colors cursor-pointer"
                    >
                        Go Back
                    </button>
                </div>
            </div>
        );
    }

    // ── Render ─────────────────────────────────────────────────────────────────

    const { user, profileTags, profileLinks, projects, reviews } = profile;

    return (
        <div className="min-h-[calc(100vh-3.5rem)] bg-[linear-gradient(135deg,#020817_0%,#0a1628_45%,#0f2154_100%)]">
            <div className="max-w-4xl mx-auto px-6 py-8 flex flex-col gap-6">
                {/* Back */}
                <button
                    onClick={() => navigate(-1)}
                    className="self-start flex items-center gap-1.5 text-xs font-medium text-slate-500 hover:text-slate-300 bg-transparent border-none p-0 cursor-pointer transition-colors"
                >
                    <svg width="14" height="14" viewBox="0 0 16 16" fill="none" className="shrink-0">
                        <path d="M10 4L6 8l4 4" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
                    </svg>
                    Back
                </button>

                {/* Profile card */}
                <div className="flex flex-col gap-5 p-6 bg-white/[0.03] border border-white/[0.06] rounded-2xl">
                    <ProfileHeader user={user} reviews={reviews} isOwnProfile={isOwnProfile} />
                    <ProfileDetails
                        tags={profileTags}
                        links={profileLinks}
                        isOwnProfile={isOwnProfile}
                        onTagRemoved={fetchProfile}
                        onTagAdded={fetchProfile}
                        onLinkAdded={fetchProfile}
                        onLinkRemoved={fetchProfile}
                    />
                </div>

                {/* Admin actions */}
                {isAdmin && !isOwnProfile && (
                    <div className="flex flex-col gap-3 p-5 bg-white/[0.03] border border-white/[0.06] rounded-xl">
                        <h3 className="text-xs font-semibold text-slate-500 uppercase tracking-wider m-0">
                            Admin Actions
                        </h3>
                        {actionError && (
                            <p className="text-xs text-red-400 m-0">{actionError}</p>
                        )}
                        <div className="flex flex-wrap gap-2">
                            <button
                                onClick={handleHardDeleteUser}
                                disabled={isHardDeleting}
                                className="px-4 py-2 text-xs font-semibold text-red-500 bg-red-500/15 border border-red-500/30 rounded-lg hover:bg-red-500/25 transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                                {isHardDeleting ? 'Deleting…' : 'Permanently Delete User'}
                            </button>
                        </div>
                    </div>
                )}

                {/* Tabs */}
                <ProfileTabs
                    activeTab={activeTab}
                    onTabChange={setActiveTab}
                    projectCount={projects.length}
                    reviewCount={reviews.length}
                />

                {/* Tab content */}
                {activeTab === 'projects' && (
                    <div className="flex flex-col gap-3">
                        {projects.length === 0 ? (
                            <div className="py-12 text-center">
                                <p className="text-sm text-slate-500 m-0">No projects yet.</p>
                            </div>
                        ) : (
                            projects.map(project => (
                                <ProfileProjectCard
                                    key={project.id}
                                    project={project}
                                    onClick={() => navigate(`/projects/${project.id}`)}
                                />
                            ))
                        )}
                    </div>
                )}

                {activeTab === 'reviews' && (
                    <div className="flex flex-col gap-3">
                        {reviews.length === 0 ? (
                            <div className="py-12 text-center">
                                <p className="text-sm text-slate-500 m-0">No reviews yet.</p>
                            </div>
                        ) : (
                            reviews.map(review => (
                                <ProfileReviewCard key={review.id} review={review} />
                            ))
                        )}
                    </div>
                )}
            </div>
        </div>
    );
}
