import { useNavigate } from 'react-router-dom';
import type { ProjectResponse } from '../../types';

interface ProjectInfoProps {
    project: ProjectResponse;
}

export default function ProjectInfo({ project }: ProjectInfoProps) {
    const navigate = useNavigate();

    const daysUntilDeadline = Math.ceil(
        (new Date(project.deadline).getTime() - Date.now()) / (1000 * 60 * 60 * 24)
    );

    const deadlineColor =
        daysUntilDeadline < 0
            ? 'text-red-400'
            : daysUntilDeadline <= 7
                ? 'text-amber-400'
                : 'text-emerald-400';

    const formatDate = (iso: string) =>
        new Date(iso).toLocaleDateString(undefined, {
            month: 'short',
            day: 'numeric',
            year: 'numeric',
        });

    return (
        <div className="flex flex-col gap-6">
            {/* Description */}
            <div>
                <h3 className="text-xs font-semibold text-slate-500 uppercase tracking-wider m-0 mb-2">
                    Description
                </h3>
                <p className="text-sm text-slate-300 m-0 leading-relaxed whitespace-pre-wrap">
                    {project.description || 'No description provided.'}
                </p>
            </div>

            {/* Tags */}
            {project.tags.length > 0 && (
                <div>
                    <h3 className="text-xs font-semibold text-slate-500 uppercase tracking-wider m-0 mb-2">
                        Tags
                    </h3>
                    <div className="flex flex-wrap gap-2">
                        {project.tags.map(tag => (
                            <span
                                key={tag.id}
                                className="px-2.5 py-1 text-xs font-medium rounded-lg bg-indigo-500/10 text-indigo-300 border border-indigo-500/15"
                            >
                                {tag.name}
                            </span>
                        ))}
                    </div>
                </div>
            )}

            {/* Meta grid */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                {/* Deadline */}
                <div className="flex flex-col gap-1 p-4 bg-white/[0.02] border border-white/[0.06] rounded-xl">
                    <span className="text-[0.65rem] font-semibold text-slate-500 uppercase tracking-wider">
                        Deadline
                    </span>
                    <span className="text-sm font-semibold text-slate-200">
                        {formatDate(project.deadline)}
                    </span>
                    <span className={`text-xs font-medium ${deadlineColor}`}>
                        {daysUntilDeadline < 0
                            ? `Overdue by ${Math.abs(daysUntilDeadline)} day${Math.abs(daysUntilDeadline) !== 1 ? 's' : ''}`
                            : daysUntilDeadline === 0
                                ? 'Due today'
                                : `${daysUntilDeadline} day${daysUntilDeadline !== 1 ? 's' : ''} remaining`}
                    </span>
                </div>

                {/* Worker */}
                <div className="flex flex-col gap-1 p-4 bg-white/[0.02] border border-white/[0.06] rounded-xl">
                    <span className="text-[0.65rem] font-semibold text-slate-500 uppercase tracking-wider">
                        Assigned Worker
                    </span>
                    {project.workerUsername ? (
                        <button
                            onClick={() => navigate(`/users/${project.workerId}`)}
                            className="text-sm font-semibold text-blue-400 hover:text-blue-300 bg-transparent border-none p-0 cursor-pointer transition-colors text-left"
                        >
                            {project.workerUsername}
                        </button>
                    ) : (
                        <span className="text-sm text-amber-500/80 font-medium">Unassigned</span>
                    )}
                </div>

                {/* Created */}
                <div className="flex flex-col gap-1 p-4 bg-white/[0.02] border border-white/[0.06] rounded-xl">
                    <span className="text-[0.65rem] font-semibold text-slate-500 uppercase tracking-wider">
                        Created
                    </span>
                    <span className="text-sm font-semibold text-slate-200">
                        {formatDate(project.createdAt)}
                    </span>
                    {project.updatedAt !== project.createdAt && (
                        <span className="text-xs text-slate-500">
                            Updated {formatDate(project.updatedAt)}
                        </span>
                    )}
                </div>
            </div>
        </div>
    );
}
