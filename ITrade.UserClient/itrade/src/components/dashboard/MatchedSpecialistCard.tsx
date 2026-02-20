import { useNavigate } from 'react-router-dom';
import type { UserMatchedResponse } from '../../types';

interface MatchedSpecialistCardProps {
    match: UserMatchedResponse;
}

/** Card for a recommended specialist with match percentage */
export default function MatchedSpecialistCard({ match }: MatchedSpecialistCardProps) {
    const navigate = useNavigate();
    const { user, matchPercentage } = match;

    const matchColor =
        matchPercentage >= 80
            ? 'from-emerald-500 to-emerald-400'
            : matchPercentage >= 50
                ? 'from-blue-500 to-blue-400'
                : 'from-amber-500 to-amber-400';

    return (
        <div
            onClick={() => navigate(`/users/${user.id}`)}
            className="flex items-center gap-4 px-5 py-4 bg-white/[0.03] border border-white/[0.06] rounded-xl hover:bg-white/[0.05] transition-colors cursor-pointer">
            {/* Avatar placeholder */}
            <div className="w-10 h-10 rounded-full bg-gradient-to-br from-blue-500/30 to-indigo-500/30 border border-white/10 flex items-center justify-center shrink-0">
                <span className="text-sm font-bold text-blue-300">
                    {user.username.charAt(0).toUpperCase()}
                </span>
            </div>

            {/* Info */}
            <div className="flex flex-col gap-0.5 min-w-0 flex-1">
                <p className="text-sm font-semibold text-slate-200 m-0 truncate">
                    {user.username}
                </p>
                <p className="text-xs text-slate-500 m-0">Specialist</p>
            </div>

            {/* Match badge */}
            <span
                className={`shrink-0 px-2.5 py-1 text-[0.65rem] font-bold rounded-full bg-gradient-to-r ${matchColor} text-white shadow-sm`}
            >
                {matchPercentage}%
            </span>
        </div>
    );
}
