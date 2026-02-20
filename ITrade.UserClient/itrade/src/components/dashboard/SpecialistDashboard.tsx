import { useState, useEffect, useCallback } from 'react';
import { dashboardService } from '../../api';
import type { DashboardSpecialistResponse, SearchResponse } from '../../types';
import DashboardSection from './DashboardSection';
import RequestCard from './RequestCard';
import ProjectCard from './ProjectCard';
import MatchedProjectCard from './MatchedProjectCard';
import SearchBar from './SearchBar';
import SearchResults from './SearchResults';

/**
 * Dashboard view for Specialist users.
 *
 * Layout:
 *  - Search bar (with filters)
 *  - "Needs Attention" — pending project invitations
 *  - "My Active Workspace" — assigned projects with status/deadline
 *  - "Recommended Opportunities" — recommended project cards
 */
export default function SpecialistDashboard() {
    const [data, setData] = useState<DashboardSpecialistResponse | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [searchResults, setSearchResults] = useState<SearchResponse | null>(null);

    const fetchDashboard = useCallback(async () => {
        setIsLoading(true);
        setError(null);
        try {
            const res = await dashboardService.getDashboard();
            setData(res as DashboardSpecialistResponse);
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
            <SearchBar
                onResults={setSearchResults}
            />

            {/* Search results overlay */}
            {searchResults && (
                <SearchResults results={searchResults} onClose={() => setSearchResults(null)} />
            )}

            {/* Only show dashboard sections when not viewing search results */}
            {!searchResults && (
                <>
                    {/* Needs Attention — Pending invitations */}
                    <DashboardSection
                        title="Needs Attention"
                        badge={data.pendingInvitations.length}
                        subtitle="New project invitations waiting for your response"
                    >
                        {data.pendingInvitations.length === 0 ? (
                            <EmptyState text="No pending invitations — you're all caught up!" />
                        ) : (
                            <div className="flex flex-col gap-2">
                                {data.pendingInvitations.map(req => (
                                    <RequestCard
                                        key={req.id}
                                        request={req}
                                        variant="invitation"
                                        onResolved={fetchDashboard}
                                    />
                                ))}
                            </div>
                        )}
                    </DashboardSection>

                    {/* My Active Workspace */}
                    <DashboardSection
                        title="My Active Workspace"
                        badge={data.activeProjects.length}
                        subtitle="Projects you're currently working on"
                    >
                        {data.activeProjects.length === 0 ? (
                            <EmptyState text="No active projects yet. Check out recommended opportunities below!" />
                        ) : (
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                                {data.activeProjects.map(project => (
                                    <ProjectCard key={project.id} project={project} />
                                ))}
                            </div>
                        )}
                    </DashboardSection>

                    {/* Recommended Opportunities */}
                    <DashboardSection
                        title="Recommended Opportunities"
                        badge={data.recommendedProjects.length}
                        subtitle="Projects that match your skills and expertise"
                    >
                        {data.recommendedProjects.length === 0 ? (
                            <EmptyState text="No recommendations right now. Update your profile tags to get better matches!" />
                        ) : (
                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                                {data.recommendedProjects.map(match => (
                                    <MatchedProjectCard key={match.project.id} match={match} />
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
