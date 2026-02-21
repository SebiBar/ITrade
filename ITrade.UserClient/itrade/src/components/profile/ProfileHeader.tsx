import { useNavigate } from 'react-router-dom';
import type { UserResponse, ReviewResponse } from '../../types';

interface ProfileHeaderProps {
    user: UserResponse;
    reviews: ReviewResponse[];
    isOwnProfile?: boolean;
}

/** Avatar, username, role badge, and average rating summary */
export default function ProfileHeader({ user, reviews, isOwnProfile }: ProfileHeaderProps) {
    const navigate = useNavigate();

    const avgRating =
        reviews.length > 0
            ? reviews.reduce((sum, r) => sum + r.rating, 0) / reviews.length
            : null;

    return (
        <div className="flex items-center gap-4">
            <div className="w-16 h-16 rounded-full bg-gradient-to-br from-blue-500/30 to-indigo-500/30 border border-blue-500/20 flex items-center justify-center shrink-0">
                <span className="text-xl font-bold text-blue-300">
                    {user.username.charAt(0).toUpperCase()}
                </span>
            </div>
            <div className="min-w-0 flex-1">
                <div className="flex items-start justify-between gap-3">
                    <h1 className="text-xl font-bold text-slate-200 m-0 break-words">
                        {user.username}
                    </h1>
                    {isOwnProfile && (
                        <button
                            onClick={() => navigate('/settings')}
                            className="hidden sm:inline-flex shrink-0 px-3 py-1.5 text-[0.65rem] font-semibold text-slate-400 bg-white/[0.04] border border-white/[0.08] rounded-lg hover:bg-white/[0.08] hover:text-slate-300 transition-all cursor-pointer"
                        >
                            Settings
                        </button>
                    )}
                </div>
                <div className="flex items-center gap-2 mt-1 flex-wrap">
                    <span className="px-2.5 py-0.5 text-[0.65rem] font-semibold rounded-full bg-blue-500/15 text-blue-400 border border-blue-500/20">
                        {user.role}
                    </span>
                    {avgRating !== null && (
                        <span className="flex items-center gap-1 text-xs text-slate-400">
                            <svg width="12" height="12" viewBox="0 0 20 20" fill="currentColor" className="text-amber-400">
                                <path d="M10 1l2.39 4.84 5.34.78-3.87 3.77.91 5.32L10 13.27l-4.77 2.44.91-5.32L2.27 6.62l5.34-.78L10 1z" />
                            </svg>
                            {avgRating.toFixed(1)} ({reviews.length} review{reviews.length !== 1 ? 's' : ''})
                        </span>
                    )}
                </div>
            </div>
        </div>
    );
}
