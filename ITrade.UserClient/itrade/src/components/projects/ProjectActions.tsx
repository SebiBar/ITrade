interface ProjectActionsProps {
    isOwner: boolean;
    isSpecialist: boolean;
    hasWorker: boolean;
    statusType: string;
    isDeleting: boolean;
    isUnassigning: boolean;
    isApplying: boolean;
    hasApplied: boolean;
    onEdit: () => void;
    onDelete: () => void;
    onUnassign: () => void;
    onShowSpecialists: () => void;
    onApply: () => void;
}

export default function ProjectActions({
    isOwner,
    isSpecialist,
    hasWorker,
    statusType,
    isDeleting,
    isUnassigning,
    isApplying,
    hasApplied,
    onEdit,
    onDelete,
    onUnassign,
    onShowSpecialists,
    onApply,
}: ProjectActionsProps) {
    const canRecommend = isOwner && !hasWorker && statusType === 'Hiring';
    const canApply = isSpecialist && !hasWorker && statusType === 'Hiring';

    if (!isOwner && !canApply) return null;

    return (
        <div className="flex flex-col gap-3 p-5 bg-white/[0.03] border border-white/[0.06] rounded-xl">
            <h3 className="text-xs font-semibold text-slate-500 uppercase tracking-wider m-0">
                Actions
            </h3>

            <div className="flex flex-wrap gap-2">
                {/* Owner actions */}
                {isOwner && (
                    <>
                        <button
                            onClick={onEdit}
                            className="px-4 py-2 text-xs font-semibold text-blue-400 bg-blue-500/10 border border-blue-500/20 rounded-lg hover:bg-blue-500/20 transition-colors cursor-pointer"
                        >
                            Edit Project
                        </button>

                        {hasWorker && (
                            <button
                                onClick={onUnassign}
                                disabled={isUnassigning}
                                className="px-4 py-2 text-xs font-semibold text-orange-400 bg-orange-500/10 border border-orange-500/20 rounded-lg hover:bg-orange-500/20 transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                                {isUnassigning ? 'Unassigning…' : 'Unassign Worker'}
                            </button>
                        )}

                        {canRecommend && (
                            <button
                                onClick={onShowSpecialists}
                                className="px-4 py-2 text-xs font-semibold text-indigo-400 bg-indigo-500/10 border border-indigo-500/20 rounded-lg hover:bg-indigo-500/20 transition-colors cursor-pointer"
                            >
                                Recommend Specialists
                            </button>
                        )}

                        <button
                            onClick={onDelete}
                            disabled={isDeleting}
                            className="px-4 py-2 text-xs font-semibold text-red-400 bg-red-500/10 border border-red-500/20 rounded-lg hover:bg-red-500/20 transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            {isDeleting ? 'Deleting…' : 'Delete Project'}
                        </button>
                    </>
                )}

                {/* Specialist apply */}
                {canApply && (
                    <button
                        onClick={onApply}
                        disabled={isApplying || hasApplied}
                        className={`px-5 py-2.5 text-sm font-semibold rounded-lg border transition-all cursor-pointer disabled:cursor-not-allowed ${hasApplied
                                ? 'text-emerald-400 bg-emerald-500/10 border-emerald-500/20'
                                : 'text-white bg-gradient-to-r from-blue-500 to-indigo-600 border-transparent hover:brightness-110 active:scale-[0.98] shadow-[0_2px_12px_rgba(79,125,255,0.3)] disabled:opacity-50'
                            }`}
                    >
                        {hasApplied ? 'Applied' : isApplying ? 'Applying…' : 'Apply for this Project'}
                    </button>
                )}
            </div>
        </div>
    );
}
