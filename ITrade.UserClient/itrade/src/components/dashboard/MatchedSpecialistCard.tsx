import { useNavigate } from 'react-router-dom';
import type { UserMatchedResponse } from '../../types';

interface MatchedSpecialistCardProps {
    match: UserMatchedResponse;
    /** Optional invite handler — when provided, an "Invite" button is shown */
    onInvite?: (userId: number) => void;
    /** Whether an invite is currently being sent for this user */
    isInviting?: boolean;
    /** Whether this user has already been invited */
    isInvited?: boolean;
}

/** Card for a recommended specialist with match percentage */
export default function MatchedSpecialistCard({
    match,
    onInvite,
    isInviting = false,
    isInvited = false,
}: MatchedSpecialistCardProps) {
    const navigate = useNavigate();
    const { user, matchPercentage } = match;

    const matchColor =
        matchPercentage >= 80
            ? 'text-emerald-400'
            : matchPercentage >= 50
                ? 'text-blue-400'
                : 'text-amber-400';

    return (
        <div
            onClick={() => navigate(`/users/${user.id}`)}
            className="flex items-center gap-4 px-5 py-4 bg-white/[0.03] border border-white/[0.06] rounded-xl hover:bg-white/[0.05] transition-colors cursor-pointer"
        >
            {/* Avatar placeholder */}
            <div className="w-10 h-10 rounded-full bg-gradient-to-br from-blue-500/30 to-indigo-500/30 border border-white/10 flex items-center justify-center shrink-0">
                <span className="text-sm font-bold text-blue-300">
                    {user.username.charAt(0).toUpperCase()}
                </span>
            </div>

            {/* Info + match % */}
            <div className="flex flex-col gap-0.5 min-w-0 flex-1">
                <div className="flex items-center gap-2">
                    <p className="text-sm font-semibold text-slate-200 m-0 truncate">
                        {user.username}
                    </p>
                    <span className={`text-[0.65rem] font-bold ${matchColor} shrink-0`}>
                        {matchPercentage}% match
                    </span>
                </div>
                <p className="text-xs text-slate-500 m-0">Specialist</p>
            </div>

            {/* Invite button (optional) */}
            {onInvite && (
                <button
                    onClick={e => {
                        e.stopPropagation();
                        if (!isInviting && !isInvited) onInvite(user.id);
                    }}
                    disabled={isInviting || isInvited}
                    className={`shrink-0 px-4 py-1.5 text-xs font-semibold rounded-lg border transition-colors cursor-pointer ${isInvited
                            ? 'text-emerald-400 bg-emerald-500/10 border-emerald-500/20 cursor-default'
                            : 'text-blue-400 bg-blue-500/10 border-blue-500/20 hover:bg-blue-500/20 disabled:opacity-50 disabled:cursor-not-allowed'
                        }`}
                >
                    {isInvited ? 'Invited' : isInviting ? 'Sending…' : 'Invite'}
                </button>
            )}
        </div>
    );
}
