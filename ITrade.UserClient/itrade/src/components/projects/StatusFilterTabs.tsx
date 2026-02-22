import type { ProjectResponse } from '../../types';

const ALL_STATUS_FILTERS = ['All', 'Hiring', 'InProgress', 'Completed', 'OnHold', 'Cancelled'] as const;
const SPECIALIST_STATUS_FILTERS = ['All', 'InProgress', 'Completed', 'OnHold', 'Cancelled'] as const;
export type StatusFilter = typeof ALL_STATUS_FILTERS[number];

const STATUS_LABELS: Record<StatusFilter, string> = {
    All: 'All',
    Hiring: 'Hiring',
    InProgress: 'In Progress',
    Completed: 'Completed',
    OnHold: 'On Hold',
    Cancelled: 'Cancelled',
};

interface StatusFilterTabsProps {
    projects: ProjectResponse[];
    activeFilter: StatusFilter;
    onFilterChange: (filter: StatusFilter) => void;
    isClient?: boolean;
}

export default function StatusFilterTabs({ projects, activeFilter, onFilterChange, isClient = true }: StatusFilterTabsProps) {
    const filters = isClient ? ALL_STATUS_FILTERS : SPECIALIST_STATUS_FILTERS;

    return (
        <div className="flex items-center gap-1 mb-6 overflow-x-auto pb-1">
            {filters.map(status => (
                <button
                    key={status}
                    onClick={() => onFilterChange(status)}
                    className={`px-4 py-2 text-xs font-medium rounded-lg transition-colors cursor-pointer border-none whitespace-nowrap ${activeFilter === status
                        ? 'bg-blue-500/20 text-blue-400 shadow-[inset_0_0_0_1px_rgba(59,130,246,0.3)]'
                        : 'text-slate-500 hover:text-slate-300 hover:bg-white/[0.04]'
                        }`}
                >
                    {STATUS_LABELS[status]}
                    {status !== 'All' && (
                        <span className="ml-1.5 text-[0.6rem] opacity-60">
                            {projects.filter(p => p.projectStatusType === status).length}
                        </span>
                    )}
                </button>
            ))}
        </div>
    );
}
