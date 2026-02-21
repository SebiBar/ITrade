import type { ProjectResponse } from '../../types';

interface ProfileProjectCardProps {
    project: ProjectResponse;
    onClick: () => void;
}

const statusColors: Record<string, string> = {
    Hiring: 'bg-amber-500/15 text-amber-400 border-amber-500/20',
    InProgress: 'bg-blue-500/15 text-blue-400 border-blue-500/20',
    Completed: 'bg-emerald-500/15 text-emerald-400 border-emerald-500/20',
    OnHold: 'bg-orange-500/15 text-orange-400 border-orange-500/20',
    Cancelled: 'bg-red-500/15 text-red-400 border-red-500/20',
};

const formatDate = (iso: string) =>
    new Date(iso).toLocaleDateString(undefined, {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
    });

/** Compact project card used on the public profile page */
export default function ProfileProjectCard({ project, onClick }: ProfileProjectCardProps) {
    const style = statusColors[project.projectStatusType] ?? 'bg-slate-500/15 text-slate-400 border-slate-500/20';

    return (
        <div
            onClick={onClick}
            className="flex flex-col gap-2 p-4 bg-white/[0.03] border border-white/[0.06] rounded-xl hover:bg-white/[0.05] hover:border-white/[0.10] transition-all cursor-pointer"
        >
            <div className="flex items-start justify-between gap-2">
                <h4 className="text-sm font-semibold text-slate-200 m-0 truncate">{project.name}</h4>
                <span className={`shrink-0 px-2 py-0.5 text-[0.6rem] font-semibold rounded-full border ${style}`}>
                    {project.projectStatusType === 'InProgress' ? 'In Progress' : project.projectStatusType}
                </span>
            </div>
            {project.description && (
                <p className="text-xs text-slate-400 m-0 line-clamp-2 leading-relaxed">{project.description}</p>
            )}
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
            <div className="flex items-center justify-between pt-1 border-t border-white/[0.04]">
                <span className="text-[0.65rem] text-slate-500">Deadline: {formatDate(project.deadline)}</span>
                <span className="text-[0.65rem] text-slate-600">Created {formatDate(project.createdAt)}</span>
            </div>
        </div>
    );
}
