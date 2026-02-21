import { useState, useEffect, useCallback } from 'react';
import { projectService } from '../api';
import { useUser } from '../context';
import type { ProjectResponse } from '../types';
import {
    CreateProjectModal,
    RecommendedPanel,
    MyProjectCard,
    StatusFilterTabs,
    type StatusFilter,
} from '../components/projects';

const STATUS_LABELS: Record<StatusFilter, string> = {
    All: 'All',
    Hiring: 'Hiring',
    InProgress: 'In Progress',
    Completed: 'Completed',
    OnHold: 'On Hold',
    Cancelled: 'Cancelled',
};

export default function MyProjectsPage() {
    const { currentUser } = useUser();
    const isClient = currentUser?.role === 'Client';

    const [projects, setProjects] = useState<ProjectResponse[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [statusFilter, setStatusFilter] = useState<StatusFilter>('All');
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [specialistsPanel, setSpecialistsPanel] = useState<{ projectId: number; projectName: string } | null>(null);

    const fetchProjects = useCallback(async () => {
        setIsLoading(true);
        setError(null);
        try {
            const data = await projectService.getUserProjects();
            setProjects(data);
        } catch {
            setError('Failed to load projects.');
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchProjects();
    }, [fetchProjects]);

    const filteredProjects =
        statusFilter === 'All'
            ? projects
            : projects.filter(p => p.projectStatusType === statusFilter);

    const handleProjectCreated = () => {
        setShowCreateModal(false);
        fetchProjects();
    };

    return (
        <div className="min-h-[calc(100vh-3.5rem)] bg-[linear-gradient(135deg,#020817_0%,#0a1628_45%,#0f2154_100%)]">
            <div className="max-w-6xl mx-auto px-6 py-8">
                {/* Header */}
                <div className="flex items-center justify-between gap-4 mb-8">
                    <div className="flex flex-col gap-1">
                        <h1 className="text-2xl font-bold text-slate-200 m-0">My Projects</h1>
                        <p className="text-sm text-slate-500 m-0">
                            {isClient
                                ? 'Manage your projects and find specialists.'
                                : 'Projects you are working on.'}
                        </p>
                    </div>
                    {isClient && (
                        <button
                            onClick={() => setShowCreateModal(true)}
                            className="px-5 py-2.5 text-sm font-semibold text-white bg-gradient-to-r from-blue-500 to-indigo-600 rounded-lg hover:brightness-110 active:scale-[0.98] transition-all cursor-pointer border-none shadow-[0_2px_12px_rgba(79,125,255,0.3)]"
                        >
                            + New Project
                        </button>
                    )}
                </div>

                {/* Status filter tabs */}
                <StatusFilterTabs
                    projects={projects}
                    activeFilter={statusFilter}
                    onFilterChange={setStatusFilter}
                />

                {/* Content */}
                {isLoading && (
                    <div className="flex items-center justify-center min-h-[40vh]">
                        <div className="flex flex-col items-center gap-3">
                            <div className="w-8 h-8 border-2 border-blue-500/30 border-t-blue-400 rounded-full animate-spin" />
                            <span className="text-sm text-slate-500">Loading projects…</span>
                        </div>
                    </div>
                )}

                {error && !isLoading && (
                    <div className="flex items-center justify-center min-h-[40vh]">
                        <div className="flex flex-col items-center gap-3 text-center">
                            <p className="text-sm text-red-400 m-0">{error}</p>
                            <button
                                onClick={fetchProjects}
                                className="px-4 py-2 text-xs font-semibold text-blue-400 bg-blue-500/10 border border-blue-500/20 rounded-lg hover:bg-blue-500/20 transition-colors cursor-pointer"
                            >
                                Retry
                            </button>
                        </div>
                    </div>
                )}

                {!isLoading && !error && filteredProjects.length === 0 && (
                    <div className="flex items-center justify-center min-h-[40vh]">
                        <div className="flex flex-col items-center gap-3 py-12 px-6 bg-white/[0.02] border border-dashed border-white/[0.06] rounded-xl text-center">
                            <p className="text-sm text-slate-500 m-0">
                                {statusFilter === 'All'
                                    ? isClient
                                        ? "You haven't created any projects yet."
                                        : "You aren't assigned to any projects yet."
                                    : `No ${STATUS_LABELS[statusFilter].toLowerCase()} projects.`}
                            </p>
                            {isClient && statusFilter === 'All' && (
                                <button
                                    onClick={() => setShowCreateModal(true)}
                                    className="mt-2 px-5 py-2 text-sm font-semibold text-blue-400 bg-blue-500/10 border border-blue-500/20 rounded-lg hover:bg-blue-500/20 transition-colors cursor-pointer"
                                >
                                    Create your first project
                                </button>
                            )}
                        </div>
                    </div>
                )}

                {!isLoading && !error && filteredProjects.length > 0 && (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                        {filteredProjects.map(project => (
                            <MyProjectCard
                                key={project.id}
                                project={project}
                                isClient={isClient}
                                onShowSpecialists={(id, name) => setSpecialistsPanel({ projectId: id, projectName: name })}
                            />
                        ))}
                    </div>
                )}
            </div>

            {/* Modals */}
            {showCreateModal && (
                <CreateProjectModal
                    onClose={() => setShowCreateModal(false)}
                    onCreated={handleProjectCreated}
                />
            )}

            {specialistsPanel && (
                <RecommendedPanel
                    projectId={specialistsPanel.projectId}
                    projectName={specialistsPanel.projectName}
                    onClose={() => setSpecialistsPanel(null)}
                />
            )}
        </div>
    );
}
