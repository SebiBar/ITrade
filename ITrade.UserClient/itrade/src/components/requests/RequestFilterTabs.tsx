import type { RequestResponse } from '../../types';

const FILTERS = ['All', 'Invitation', 'Application'] as const;
export type RequestFilter = typeof FILTERS[number];

const FILTER_LABELS: Record<RequestFilter, string> = {
    All: 'All',
    Invitation: 'Invitations',
    Application: 'Applications',
};

interface RequestFilterTabsProps {
    requests: RequestResponse[];
    activeFilter: RequestFilter;
    onFilterChange: (filter: RequestFilter) => void;
}

export default function RequestFilterTabs({
    requests,
    activeFilter,
    onFilterChange,
}: RequestFilterTabsProps) {
    return (
        <div className="flex items-center gap-1 mb-6 overflow-x-auto pb-1">
            {FILTERS.map(filter => (
                <button
                    key={filter}
                    onClick={() => onFilterChange(filter)}
                    className={`px-4 py-2 text-xs font-medium rounded-lg transition-colors cursor-pointer border-none whitespace-nowrap ${activeFilter === filter
                            ? 'bg-blue-500/20 text-blue-400 shadow-[inset_0_0_0_1px_rgba(59,130,246,0.3)]'
                            : 'text-slate-500 hover:text-slate-300 hover:bg-white/[0.04]'
                        }`}
                >
                    {FILTER_LABELS[filter]}
                    {filter !== 'All' && (
                        <span className="ml-1.5 text-[0.6rem] opacity-60">
                            {requests.filter(r => r.requestType === filter).length}
                        </span>
                    )}
                </button>
            ))}
        </div>
    );
}
