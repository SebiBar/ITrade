import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { projectService, requestService } from '../api';
import { ProjectRequestType } from '../types';
import { useUser } from '../context';
import type { ProjectResponse } from '../types';
import {
    ProjectHeader,
    ProjectInfo,
    ProjectActions,
    EditProjectModal,
    RecommendedPanel,
} from '../components/projects';

export default function ProjectDetailPage() {
    const { projectId } = useParams<{ projectId: string }>();
    const navigate = useNavigate();
    const { currentUser } = useUser();

    const [project, setProject] = useState<ProjectResponse | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // Action states
    const [showEditModal, setShowEditModal] = useState(false);
    const [showSpecialistsPanel, setShowSpecialistsPanel] = useState(false);
    const [isDeleting, setIsDeleting] = useState(false);
    const [isUnassigning, setIsUnassigning] = useState(false);
    const [isApplying, setIsApplying] = useState(false);
    const [hasApplied, setHasApplied] = useState(false);
    const [actionError, setActionError] = useState<string | null>(null);

    const fetchProject = useCallback(async () => {
        if (!projectId) return;
        setIsLoading(true);
        setError(null);
        try {
            const data = await projectService.getProject(Number(projectId));
            setProject(data);
        } catch {
            setError('Failed to load project.');
        } finally {
            setIsLoading(false);
        }
    }, [projectId]);

    useEffect(() => {
        fetchProject();
    }, [fetchProject]);

    const isOwner = currentUser?.id === project?.ownerId;
    const isSpecialist = currentUser?.role === 'Specialist';

    // Check if specialist has already applied
    useEffect(() => {
        if (!isSpecialist || !projectId) return;
        let cancelled = false;
        requestService
            .hasApplied(Number(projectId))
            .then(applied => {
                if (!cancelled) setHasApplied(applied);
            })
            .catch(() => { /* ignore */ });
        return () => { cancelled = true; };
    }, [isSpecialist, projectId]);

    // ── Handlers ──────────────────────────────────────────────────────────────

    const handleDelete = async () => {
        if (!project) return;
        const confirmed = window.confirm(
            `Are you sure you want to delete "${project.name}"? This action can be undone by an admin.`
        );
        if (!confirmed) return;
        setIsDeleting(true);
        setActionError(null);
        try {
            await projectService.softDeleteProject(project.id);
            navigate('/projects', { replace: true });
        } catch {
            setActionError('Failed to delete project.');
        } finally {
            setIsDeleting(false);
        }
    };

    const handleUnassign = async () => {
        if (!project) return;
        const confirmed = window.confirm(
            `Unassign ${project.workerUsername} from this project?`
        );
        if (!confirmed) return;
        setIsUnassigning(true);
        setActionError(null);
        try {
            await projectService.unassignWorker(project.id);
            await fetchProject();
        } catch {
            setActionError('Failed to unassign worker.');
        } finally {
            setIsUnassigning(false);
        }
    };

    const handleApply = async () => {
        if (!project || !currentUser) return;
        setIsApplying(true);
        setActionError(null);
        try {
            await requestService.createRequest({
                receiverId: project.ownerId,
                projectId: project.id,
                requestType: ProjectRequestType.Application,
            });
            setHasApplied(true);
        } catch {
            setActionError('Failed to submit application.');
        } finally {
            setIsApplying(false);
        }
    };

    const handleUpdated = () => {
        setShowEditModal(false);
        fetchProject();
    };

    // ── Loading / Error states ────────────────────────────────────────────────

    if (isLoading) {
        return (
            <div className="min-h-[calc(100vh-3.5rem)] bg-[linear-gradient(135deg,#020817_0%,#0a1628_45%,#0f2154_100%)] flex items-center justify-center">
                <div className="flex flex-col items-center gap-3">
                    <div className="w-8 h-8 border-2 border-blue-500/30 border-t-blue-400 rounded-full animate-spin" />
                    <span className="text-sm text-slate-500">Loading project…</span>
                </div>
            </div>
        );
    }

    if (error || !project) {
        return (
            <div className="min-h-[calc(100vh-3.5rem)] bg-[linear-gradient(135deg,#020817_0%,#0a1628_45%,#0f2154_100%)] flex items-center justify-center">
                <div className="flex flex-col items-center gap-3 text-center">
                    <p className="text-sm text-red-400 m-0">{error ?? 'Project not found.'}</p>
                    <button
                        onClick={() => navigate(-1)}
                        className="px-4 py-2 text-xs font-semibold text-blue-400 bg-blue-500/10 border border-blue-500/20 rounded-lg hover:bg-blue-500/20 transition-colors cursor-pointer"
                    >
                        Go Back
                    </button>
                </div>
            </div>
        );
    }

    // ── Render ─────────────────────────────────────────────────────────────────

    return (
        <div className="min-h-[calc(100vh-3.5rem)] bg-[linear-gradient(135deg,#020817_0%,#0a1628_45%,#0f2154_100%)]">
            <div className="max-w-4xl mx-auto px-6 py-8 flex flex-col gap-6">
                {/* Back button */}
                <button
                    onClick={() => navigate(-1)}
                    className="self-start flex items-center gap-1.5 text-xs font-medium text-slate-500 hover:text-slate-300 bg-transparent border-none p-0 cursor-pointer transition-colors"
                >
                    <svg width="14" height="14" viewBox="0 0 16 16" fill="none" className="shrink-0">
                        <path d="M10 4L6 8l4 4" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
                    </svg>
                    Back
                </button>

                {/* Action error banner */}
                {actionError && (
                    <div className="px-4 py-2.5 text-sm text-red-400 bg-red-500/10 border border-red-500/20 rounded-lg">
                        {actionError}
                    </div>
                )}

                {/* Header */}
                <ProjectHeader
                    projectName={project.name}
                    statusType={project.projectStatusType}
                    ownerUsername={project.ownerUsername}
                    ownerId={project.ownerId}
                    isOwner={isOwner}
                />

                {/* Divider */}
                <div className="h-px bg-white/[0.06]" />

                {/* Info section */}
                <ProjectInfo project={project} />

                {/* Actions */}
                <ProjectActions
                    isOwner={isOwner}
                    isSpecialist={isSpecialist}
                    hasWorker={!!project.workerId}
                    statusType={project.projectStatusType}
                    isDeleting={isDeleting}
                    isUnassigning={isUnassigning}
                    isApplying={isApplying}
                    hasApplied={hasApplied}
                    onEdit={() => setShowEditModal(true)}
                    onDelete={handleDelete}
                    onUnassign={handleUnassign}
                    onShowSpecialists={() => setShowSpecialistsPanel(true)}
                    onApply={handleApply}
                />
            </div>

            {/* Modals */}
            {showEditModal && (
                <EditProjectModal
                    project={project}
                    onClose={() => setShowEditModal(false)}
                    onUpdated={handleUpdated}
                />
            )}

            {showSpecialistsPanel && (
                <RecommendedPanel
                    projectId={project.id}
                    projectName={project.name}
                    onClose={() => setShowSpecialistsPanel(false)}
                />
            )}
        </div>
    );
}
