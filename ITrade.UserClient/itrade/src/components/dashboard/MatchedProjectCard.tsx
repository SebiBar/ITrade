import { useNavigate } from 'react-router-dom';
import type { ProjectMatchedResponse } from '../../types';

interface MatchedProjectCardProps {
    match: ProjectMatchedResponse;
}

/** Card for a recommended project with match percentage badge */
export default function MatchedProjectCard({ match }: MatchedProjectCardProps) {
    const navigate = useNavigate();
    const { project, matchPercentage } = match;

    const formatDate = (iso: string) =>
        new Date(iso).toLocaleDateString(undefined, {
            month: 'short',
            day: 'numeric',
            year: 'numeric',
        });

    const matchColor =
        matchPercentage >= 80
            ? 'from-emerald-500 to-emerald-400'
            : matchPercentage >= 50
                ? 'from-blue-500 to-blue-400'
                : 'from-amber-500 to-amber-400';

    return (
        <div
            onClick={() => navigate(`/projects/${project.id}`)}
            className="flex flex-col gap-3 p-5 bg-white/[0.03] border border-white/[0.06] rounded-xl hover:bg-white/[0.05] hover:border-white/[0.10] transition-all group cursor-pointer">
            {/* Header */}
            <div className="flex items-start justify-between gap-3">
                <div className="min-w-0">
                    <h3 className="text-sm font-semibold text-slate-200 m-0 truncate">
                        {project.name}
                    </h3>
                    <p className="text-xs text-slate-500 m-0 mt-0.5">
                        by {project.ownerUsername}
                    </p>
                </div>
                <span
                    className={`shrink-0 px-2.5 py-1 text-[0.65rem] font-bold rounded-full bg-gradient-to-r ${matchColor} text-white shadow-sm`}
                >
                    {matchPercentage}% match
                </span>
            </div>

            {/* Description */}
            {project.description && (
                <p className="text-xs text-slate-400 m-0 line-clamp-2 leading-relaxed">
                    {project.description}
                </p>
            )}

            {/* Tags */}
            {project.tags.length > 0 && (
                <div className="flex flex-wrap gap-1.5">
                    {project.tags.map(tag => (
                        <span
                            key={tag.id}
                            className="px-2 py-0.5 text-[0.6rem] font-medium rounded-md bg-indigo-500/10 text-indigo-300 border border-indigo-500/15"
                        >
                            {tag.name}
                        </span>
                    ))}
                </div>
            )}

            {/* Footer */}
            <div className="flex items-center justify-between pt-1 border-t border-white/[0.04]">
                <span className="text-[0.65rem] text-slate-500">
                    Deadline: {formatDate(project.deadline)}
                </span>
                <span className="text-[0.65rem] text-slate-600">
                    Posted {formatDate(project.createdAt)}
                </span>
            </div>
        </div>
    );
}
