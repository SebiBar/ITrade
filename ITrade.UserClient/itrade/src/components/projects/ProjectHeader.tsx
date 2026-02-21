import { useNavigate } from 'react-router-dom';

interface ProjectHeaderProps {
    projectName: string;
    statusType: string;
    ownerUsername: string;
    ownerId: number;
    isOwner: boolean;
}

const statusColors: Record<string, string> = {
    Hiring: 'bg-amber-500/15 text-amber-400 border-amber-500/20',
    InProgress: 'bg-blue-500/15 text-blue-400 border-blue-500/20',
    Completed: 'bg-emerald-500/15 text-emerald-400 border-emerald-500/20',
    OnHold: 'bg-orange-500/15 text-orange-400 border-orange-500/20',
    Cancelled: 'bg-red-500/15 text-red-400 border-red-500/20',
};

export default function ProjectHeader({
    projectName,
    statusType,
    ownerUsername,
    ownerId,
    isOwner,
}: ProjectHeaderProps) {
    const navigate = useNavigate();
    const statusStyle = statusColors[statusType] ?? 'bg-slate-500/15 text-slate-400 border-slate-500/20';

    return (
        <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
            <div className="min-w-0">
                <h1 className="text-2xl font-bold text-slate-200 m-0 break-words">
                    {projectName}
                </h1>
                <p className="text-sm text-slate-500 m-0 mt-1">
                    {isOwner ? (
                        'You own this project'
                    ) : (
                        <>
                            Owned by{' '}
                            <button
                                onClick={() => navigate(`/users/${ownerId}`)}
                                className="text-blue-400 hover:text-blue-300 bg-transparent border-none p-0 cursor-pointer font-medium transition-colors"
                            >
                                {ownerUsername}
                            </button>
                        </>
                    )}
                </p>
            </div>
            <span
                className={`self-start shrink-0 px-3.5 py-1.5 text-xs font-semibold rounded-full border ${statusStyle}`}
            >
                {statusType === 'InProgress' ? 'In Progress' : statusType}
            </span>
        </div>
    );
}
