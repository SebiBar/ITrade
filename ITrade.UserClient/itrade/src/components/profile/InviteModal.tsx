import { useState, useEffect, useRef } from 'react';
import { projectService, requestService } from '../../api';
import { ProjectRequestType } from '../../types/enums';
import type { ProjectSummarizedResponse } from '../../types/responses';

interface InviteModalProps {
    toBeInvitedId: number;
    onClose: () => void;
}

export default function InviteModal({ toBeInvitedId, onClose }: InviteModalProps) {
    const [projects, setProjects] = useState<ProjectSummarizedResponse[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [invitingId, setInvitingId] = useState<number | null>(null);
    const panelRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const fetchProjects = async () => {
            setIsLoading(true);
            try {
                const data = await projectService.getUserOpenProjects(toBeInvitedId);
                setProjects(data);
            } catch (err) {
                setError('Failed to load open projects.');
            } finally {
                setIsLoading(false);
            }
        };

        fetchProjects();
    }, [toBeInvitedId]);

    // Close on click outside
    useEffect(() => {
        const handleClick = (e: MouseEvent) => {
            if (panelRef.current && !panelRef.current.contains(e.target as Node)) {
                onClose();
            }
        };
        document.addEventListener('mousedown', handleClick);
        return () => document.removeEventListener('mousedown', handleClick);
    }, [onClose]);

    const handleInvite = async (projectId: number) => {
        setInvitingId(projectId);
        try {
            await requestService.createRequest({
                projectId,
                receiverId: toBeInvitedId,
                requestType: ProjectRequestType.Invitation,
                message: 'I would like to invite you to this project.',
            });
            // Optionally, remove the project from the list or show a success message
            setProjects(prev => prev.filter(p => p.id !== projectId));
        } catch (err) {
            alert('Failed to send invite.');
        } finally {
            setInvitingId(null);
        }
    };

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
            <div ref={panelRef} className="w-full max-w-md bg-[#0a1628] border border-white/10 rounded-2xl shadow-2xl flex flex-col overflow-hidden">
                {/* Header */}
                <div className="flex items-center justify-between p-4 border-b border-white/5 bg-white/5">
                    <h2 className="text-lg font-semibold text-slate-200 m-0">
                        Invite to Project
                    </h2>
                    <button
                        onClick={onClose}
                        className="p-1.5 text-slate-400 hover:text-red-400 hover:bg-white/5 rounded-lg transition-colors cursor-pointer border-none bg-transparent"
                    >
                        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                            <path d="M18 6L6 18M6 6l12 12" strokeLinecap="round" strokeLinejoin="round" />
                        </svg>
                    </button>
                </div>

                {/* Content */}
                <div className="p-4 flex flex-col gap-3 min-h-[50vh] max-h-[60vh] overflow-y-auto">
                    {isLoading ? (
                        <div className="flex-1 flex items-center justify-center">
                            <div className="w-6 h-6 border-2 border-blue-500/30 border-t-blue-400 rounded-full animate-spin" />
                        </div>
                    ) : error ? (
                        <p className="text-sm text-red-400 text-center m-0 py-8">{error}</p>
                    ) : projects.length === 0 ? (
                        <p className="text-sm text-slate-500 text-center m-0 py-8">
                            No open projects available to invite this user to.
                        </p>
                    ) : (
                        projects.map(project => (
                            <div
                                key={project.id}
                                className="flex items-center justify-between p-3 rounded-xl bg-white/[0.03] border border-white/[0.06]"
                            >
                                <div className="flex flex-col gap-1 min-w-0 pr-4">
                                    <h4 className="text-sm font-medium text-slate-200 m-0 truncate">
                                        {project.name}
                                    </h4>
                                </div>
                                <button
                                    onClick={() => handleInvite(project.id)}
                                    disabled={invitingId === project.id}
                                    className="shrink-0 px-3 py-1.5 text-xs font-semibold text-blue-400 bg-blue-500/10 border border-blue-500/20 rounded-lg hover:bg-blue-500/20 transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                                >
                                    {invitingId === project.id ? 'Inviting...' : 'Invite'}
                                </button>
                            </div>
                        ))
                    )}
                </div>
            </div>
        </div>
    );
}
