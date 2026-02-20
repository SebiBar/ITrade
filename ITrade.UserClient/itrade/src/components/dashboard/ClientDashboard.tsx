import { useState, useEffect, useCallback } from 'react';
import { dashboardService } from '../../api';
import type { DashboardClientResponse, SearchResponse } from '../../types';
import DashboardSection from './DashboardSection';
import RequestCard from './RequestCard';
import MatchedSpecialistCard from './MatchedSpecialistCard';
import SearchBar from './SearchBar';
import SearchResults from './SearchResults';

/**
 * Dashboard view for Client users.
 *
 * Layout:
 *  - Search bar (with filters)
 *  - "Needs Attention" — new project applications
 *  - "Top Matches for Your Open Roles" — per project, recommended specialists
 */
export default function ClientDashboard() {
    const [data, setData] = useState<DashboardClientResponse | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [searchResults, setSearchResults] = useState<SearchResponse | null>(null);

    const fetchDashboard = useCallback(async () => {
        setIsLoading(true);
        setError(null);
        try {
            const res = await dashboardService.getDashboard();
            setData(res as DashboardClientResponse);
        } catch {
            setError('Failed to load dashboard data.');
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchDashboard();
    }, [fetchDashboard]);

    if (isLoading) {
        return (
            <div className="flex items-center justify-center min-h-[60vh]">
                <div className="flex flex-col items-center gap-3">
                    <div className="w-8 h-8 border-2 border-blue-500/30 border-t-blue-400 rounded-full animate-spin" />
                    <span className="text-sm text-slate-500">Loading dashboard…</span>
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="flex items-center justify-center min-h-[60vh]">
                <div className="flex flex-col items-center gap-3 text-center">
                    <p className="text-sm text-red-400 m-0">{error}</p>
                    <button
                        onClick={fetchDashboard}
                        className="px-4 py-2 text-xs font-semibold text-blue-400 bg-blue-500/10 border border-blue-500/20 rounded-lg hover:bg-blue-500/20 transition-colors cursor-pointer"
                    >
                        Retry
                    </button>
                </div>
            </div>
        );
    }

    if (!data) return null;

    return (
        <div className="flex flex-col gap-8">
            {/* Search */}
            <SearchBar onResults={setSearchResults} />

            {/* Search results overlay */}
            {searchResults && (
                <SearchResults results={searchResults} onClose={() => setSearchResults(null)} />
            )}

            {/* Dashboard sections (hidden during search) */}
            {!searchResults && (
                <>
                    {/* Needs Attention — Pending applications */}
                    <DashboardSection
                        title="Needs Attention"
                        badge={data.pendingApplications.length}
                        subtitle="New applications from specialists for your projects"
                    >
                        {data.pendingApplications.length === 0 ? (
                            <EmptyState text="No pending applications — you're all caught up!" />
                        ) : (
                            <div className="flex flex-col gap-2">
                                {data.pendingApplications.map(req => (
                                    <RequestCard
                                        key={req.id}
                                        request={req}
                                        variant="application"
                                        onResolved={fetchDashboard}
                                    />
                                ))}
                            </div>
                        )}
                    </DashboardSection>

                    {/* Top Matches for Open Roles */}
                    <DashboardSection
                        title="Top Matches for Your Open Roles"
                        badge={data.openProjectsWithMatches.length}
                        subtitle="Recommended specialists for your projects that don't have an assigned worker yet"
                    >
                        {data.openProjectsWithMatches.length === 0 ? (
                            <EmptyState text="No open projects without a worker. Create a new project to get specialist recommendations!" />
                        ) : (
                            <div className="flex flex-col gap-6">
                                {data.openProjectsWithMatches.map(({ project, recommendedSpecialists }) => (
                                    <div
                                        key={project.id}
                                        className="flex flex-col gap-3 p-5 bg-white/[0.02] border border-white/[0.06] rounded-xl"
                                    >
                                        {/* Project header */}
                                        <div className="flex items-center justify-between gap-3">
                                            <div className="min-w-0">
                                                <h3 className="text-sm font-semibold text-slate-200 m-0 truncate">
                                                    {project.name}
                                                </h3>
                                                {project.description && (
                                                    <p className="text-xs text-slate-500 m-0 mt-0.5 truncate">
                                                        {project.description}
                                                    </p>
                                                )}
                                            </div>
                                            {/* Tags */}
                                            {project.tags.length > 0 && (
                                                <div className="flex flex-wrap gap-1 shrink-0">
                                                    {project.tags.slice(0, 3).map(tag => (
                                                        <span
                                                            key={tag.id}
                                                            className="px-2 py-0.5 text-[0.6rem] font-medium rounded-md bg-indigo-500/10 text-indigo-300 border border-indigo-500/15"
                                                        >
                                                            {tag.name}
                                                        </span>
                                                    ))}
                                                    {project.tags.length > 3 && (
                                                        <span className="px-2 py-0.5 text-[0.6rem] font-medium rounded-md bg-white/5 text-slate-500">
                                                            +{project.tags.length - 3}
                                                        </span>
                                                    )}
                                                </div>
                                            )}
                                        </div>

                                        {/* Recommended specialists */}
                                        {recommendedSpecialists.length === 0 ? (
                                            <p className="text-xs text-slate-500 m-0 py-2">
                                                No specialist matches found for this project yet.
                                            </p>
                                        ) : (
                                            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2">
                                                {recommendedSpecialists.map(match => (
                                                    <MatchedSpecialistCard
                                                        key={match.user.id}
                                                        match={match}
                                                    />
                                                ))}
                                            </div>
                                        )}
                                    </div>
                                ))}
                            </div>
                        )}
                    </DashboardSection>
                </>
            )}
        </div>
    );
}

function EmptyState({ text }: { text: string }) {
    return (
        <div className="flex items-center justify-center py-8 px-4 bg-white/[0.02] border border-dashed border-white/[0.06] rounded-xl">
            <p className="text-sm text-slate-500 m-0 text-center">{text}</p>
        </div>
    );
}
