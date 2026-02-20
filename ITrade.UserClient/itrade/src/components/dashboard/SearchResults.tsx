import { useNavigate } from 'react-router-dom';
import type { SearchResponse } from '../../types';
import ProjectCard from './ProjectCard';

interface SearchResultsProps {
    results: SearchResponse;
    onClose: () => void;
}

/** Overlay showing search results for projects and users */
export default function SearchResults({ results, onClose }: SearchResultsProps) {
    const navigate = useNavigate();
    const hasProjects = results.projects.length > 0;
    const hasUsers = results.users.length > 0;
    const isEmpty = !hasProjects && !hasUsers;

    return (
        <div className="flex flex-col gap-5 p-6 bg-white/[0.02] border border-white/[0.06] rounded-2xl">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                    <h2 className="text-lg font-semibold text-slate-200 m-0">Search Results</h2>
                    <span className="text-xs text-slate-500">
                        {results.totalProjects + results.totalUsers} total
                    </span>
                </div>
                <button
                    onClick={onClose}
                    className="p-1.5 text-slate-500 hover:text-slate-300 rounded-lg hover:bg-white/[0.05] transition-colors cursor-pointer"
                    title="Close results"
                >
                    <svg className="w-4 h-4" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
                        <line x1="18" y1="6" x2="6" y2="18" />
                        <line x1="6" y1="6" x2="18" y2="18" />
                    </svg>
                </button>
            </div>

            {isEmpty && (
                <p className="text-sm text-slate-500 text-center py-6 m-0">
                    No results found. Try adjusting your search or filters.
                </p>
            )}

            {/* Projects */}
            {hasProjects && (
                <div className="flex flex-col gap-3">
                    <h3 className="text-sm font-semibold text-slate-400 m-0">
                        Projects ({results.totalProjects})
                    </h3>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                        {results.projects.map(p => (
                            <ProjectCard key={p.id} project={p} />
                        ))}
                    </div>
                </div>
            )}

            {/* Users */}
            {hasUsers && (
                <div className="flex flex-col gap-3">
                    <h3 className="text-sm font-semibold text-slate-400 m-0">
                        Users ({results.totalUsers})
                    </h3>
                    <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-3">
                        {results.users.map(u => (
                            <div
                                key={u.id}
                                onClick={() => navigate(`/users/${u.id}`)}
                                className="flex items-center gap-3 px-4 py-3 bg-white/[0.03] border border-white/[0.06] rounded-xl hover:bg-white/[0.05] transition-colors cursor-pointer"
                            >
                                <div className="w-9 h-9 rounded-full bg-gradient-to-br from-blue-500/30 to-indigo-500/30 border border-white/10 flex items-center justify-center shrink-0">
                                    <span className="text-xs font-bold text-blue-300">
                                        {u.username.charAt(0).toUpperCase()}
                                    </span>
                                </div>
                                <div className="min-w-0">
                                    <p className="text-sm font-semibold text-slate-200 m-0 truncate">
                                        {u.username}
                                    </p>
                                    <p className="text-xs text-slate-500 m-0">{u.role}</p>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
}
