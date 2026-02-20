import { useNavigate } from 'react-router-dom';
import type { ProjectResponse } from '../../types';

interface ProjectCardProps {
    project: ProjectResponse;
}

/** Displays an active project with status, deadline, and tags */
export default function ProjectCard({ project }: ProjectCardProps) {
    const navigate = useNavigate();
    const statusColors: Record<string, string> = {
        Hiring: 'bg-amber-500/15 text-amber-400 border-amber-500/20',
        InProgress: 'bg-blue-500/15 text-blue-400 border-blue-500/20',
        Completed: 'bg-emerald-500/15 text-emerald-400 border-emerald-500/20',
        OnHold: 'bg-orange-500/15 text-orange-400 border-orange-500/20',
        Cancelled: 'bg-red-500/15 text-red-400 border-red-500/20',
    };

    const statusStyle = statusColors[project.projectStatusType] ?? 'bg-slate-500/15 text-slate-400 border-slate-500/20';

    const daysUntilDeadline = Math.ceil(
        (new Date(project.deadline).getTime() - Date.now()) / (1000 * 60 * 60 * 24)
    );

    const deadlineColor =
        daysUntilDeadline < 0
            ? 'text-red-400'
            : daysUntilDeadline <= 7
                ? 'text-amber-400'
                : 'text-slate-500';

    const formatDate = (iso: string) =>
        new Date(iso).toLocaleDateString(undefined, {
            month: 'short',
            day: 'numeric',
            year: 'numeric',
        });

    return (
        <div
            onClick={() => navigate(`/projects/${project.id}`)}
            className="flex flex-col gap-3 p-5 bg-white/[0.03] border border-white/[0.06] rounded-xl hover:bg-white/[0.05] transition-colors cursor-pointer">
            {/* Header row */}
            <div className="flex items-start justify-between gap-3">
                <div className="min-w-0">
                    <h3 className="text-sm font-semibold text-slate-200 m-0 truncate">
                        {project.name}
                    </h3>
                    {project.ownerUsername && (
                        <p className="text-xs text-slate-500 m-0 mt-0.5">
                            by {project.ownerUsername}
                        </p>
                    )}
                </div>
                <span className={`shrink-0 px-2.5 py-1 text-[0.65rem] font-semibold rounded-full border ${statusStyle}`}>
                    {project.projectStatusType}
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
                <span className={`text-[0.65rem] font-medium ${deadlineColor}`}>
                    {daysUntilDeadline < 0
                        ? `Overdue by ${Math.abs(daysUntilDeadline)}d`
                        : daysUntilDeadline === 0
                            ? 'Due today'
                            : `${daysUntilDeadline}d remaining`}
                </span>
                <span className="text-[0.65rem] text-slate-600">
                    Due {formatDate(project.deadline)}
                </span>
            </div>
        </div>
    );
}
