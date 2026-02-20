import { useState, useEffect, useCallback } from 'react';
import { dashboardService, tagService } from '../../api';
import type { DashboardAdminResponse, Tag, SearchResponse } from '../../types';
import DashboardSection from './DashboardSection';
import SearchBar from './SearchBar';
import SearchResults from './SearchResults';

/**
 * Dashboard view for Admin users.
 *
 * Layout:
 *  - Search bar (with filters)
 *  - "Platform Overview" — notification count summary
 *  - "Tag Management" — create / delete tags
 */
export default function AdminDashboard() {
    const [data, setData] = useState<DashboardAdminResponse | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [searchResults, setSearchResults] = useState<SearchResponse | null>(null);

    // Tag management state
    const [newTagName, setNewTagName] = useState('');
    const [isCreating, setIsCreating] = useState(false);
    const [tagError, setTagError] = useState<string | null>(null);
    const [deletingIds, setDeletingIds] = useState<Set<number>>(new Set());

    const fetchDashboard = useCallback(async () => {
        setIsLoading(true);
        setError(null);
        try {
            const res = await dashboardService.getDashboard();
            setData(res as DashboardAdminResponse);
        } catch {
            setError('Failed to load dashboard data.');
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchDashboard();
    }, [fetchDashboard]);

    const handleCreateTag = async () => {
        const name = newTagName.trim();
        if (!name) return;

        setIsCreating(true);
        setTagError(null);
        try {
            await tagService.createTag(name);
            setNewTagName('');
            await fetchDashboard();
        } catch {
            setTagError('Failed to create tag. It may already exist.');
        } finally {
            setIsCreating(false);
        }
    };

    const handleDeleteTag = async (tag: Tag) => {
        setDeletingIds(prev => new Set(prev).add(tag.id));
        try {
            await tagService.deleteTag(tag.id);
            setData(prev =>
                prev
                    ? { ...prev, tags: prev.tags.filter(t => t.id !== tag.id) }
                    : prev
            );
        } catch {
            setTagError(`Failed to delete "${tag.name}".`);
        } finally {
            setDeletingIds(prev => {
                const next = new Set(prev);
                next.delete(tag.id);
                return next;
            });
        }
    };

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

            {!searchResults && (
                <>
                    {/* Platform Overview */}
                    <DashboardSection
                        title="Platform Overview"
                        subtitle="Quick look at platform activity"
                    >
                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                            <StatCard
                                label="Unread Notifications"
                                value={data.unreadNotificationCount}
                                icon={
                                    <svg className="w-5 h-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
                                        <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
                                        <path d="M13.73 21a2 2 0 0 1-3.46 0" />
                                    </svg>
                                }
                            />
                            <StatCard
                                label="Total Tags"
                                value={data.tags.length}
                                icon={
                                    <svg className="w-5 h-5" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
                                        <path d="M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z" />
                                        <line x1="7" y1="7" x2="7.01" y2="7" />
                                    </svg>
                                }
                            />
                        </div>
                    </DashboardSection>

                    {/* Tag Management */}
                    <DashboardSection
                        title="Tag Management"
                        badge={data.tags.length}
                        subtitle="Create and manage platform tags used for project and specialist matching"
                    >
                        {/* Create tag */}
                        <div className="flex items-center gap-2">
                            <input
                                type="text"
                                value={newTagName}
                                onChange={e => setNewTagName(e.target.value)}
                                onKeyDown={e => e.key === 'Enter' && handleCreateTag()}
                                placeholder="New tag name…"
                                className="flex-1 px-4 py-2.5 bg-white/[0.05] border border-white/10 rounded-xl text-sm text-slate-200 placeholder:text-slate-600 outline-none focus:border-blue-500/50 focus:ring-2 focus:ring-blue-500/15 transition-all"
                            />
                            <button
                                onClick={handleCreateTag}
                                disabled={isCreating || !newTagName.trim()}
                                className="px-5 py-2.5 bg-gradient-to-br from-[#3b5bdb] to-[#4f7dff] text-white text-sm font-semibold rounded-xl border-0 shadow-[0_4px_20px_rgba(79,125,255,0.25)] hover:opacity-90 transition-opacity cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                                {isCreating ? 'Creating…' : 'Create Tag'}
                            </button>
                        </div>

                        {tagError && (
                            <p className="text-xs text-red-400 m-0">{tagError}</p>
                        )}

                        {/* Tags list */}
                        {data.tags.length === 0 ? (
                            <EmptyState text="No tags yet. Create your first tag above!" />
                        ) : (
                            <div className="flex flex-wrap gap-2">
                                {data.tags.map(tag => (
                                    <div
                                        key={tag.id}
                                        className="group flex items-center gap-2 px-3 py-1.5 bg-indigo-500/10 border border-indigo-500/15 rounded-lg"
                                    >
                                        <span className="text-xs font-medium text-indigo-300">
                                            {tag.name}
                                        </span>
                                        <button
                                            onClick={() => handleDeleteTag(tag)}
                                            disabled={deletingIds.has(tag.id)}
                                            className="p-0.5 text-slate-600 hover:text-red-400 transition-colors cursor-pointer opacity-0 group-hover:opacity-100 disabled:opacity-50 disabled:cursor-not-allowed"
                                            title={`Delete "${tag.name}"`}
                                        >
                                            <svg className="w-3 h-3" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2.5} strokeLinecap="round" strokeLinejoin="round">
                                                <line x1="18" y1="6" x2="6" y2="18" />
                                                <line x1="6" y1="6" x2="18" y2="18" />
                                            </svg>
                                        </button>
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

function StatCard({ label, value, icon }: { label: string; value: number; icon: React.ReactNode }) {
    return (
        <div className="flex items-center gap-4 p-5 bg-white/[0.03] border border-white/[0.06] rounded-xl">
            <div className="w-10 h-10 rounded-xl bg-blue-500/10 border border-blue-500/15 flex items-center justify-center text-blue-400 shrink-0">
                {icon}
            </div>
            <div className="flex flex-col gap-0.5">
                <span className="text-2xl font-bold text-slate-200">{value}</span>
                <span className="text-xs text-slate-500">{label}</span>
            </div>
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
