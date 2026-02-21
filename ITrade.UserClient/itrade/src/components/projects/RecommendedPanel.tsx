import { useState, useEffect, useRef, useCallback } from 'react';
import { discoveryService, requestService } from '../../api';
import { ProjectRequestType } from '../../types';
import type { UserMatchedResponse } from '../../types';
import { MatchedSpecialistCard } from '../dashboard';

interface RecommendedPanelProps {
    projectId: number;
    projectName: string;
    onClose: () => void;
}

export default function RecommendedPanel({ projectId, projectName, onClose }: RecommendedPanelProps) {
    const [specialists, setSpecialists] = useState<UserMatchedResponse[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [invitingIds, setInvitingIds] = useState<Set<number>>(new Set());
    const [invitedIds, setInvitedIds] = useState<Set<number>>(new Set());
    const panelRef = useRef<HTMLDivElement>(null);

    // Fetch recommended specialists
    useEffect(() => {
        let cancelled = false;
        const fetch = async () => {
            setIsLoading(true);
            setError(null);
            try {
                const data = await discoveryService.getRecommendedSpecialistsForProject(projectId);
                if (!cancelled) setSpecialists(data);
            } catch {
                if (!cancelled) setError('Failed to load recommendations.');
            } finally {
                if (!cancelled) setIsLoading(false);
            }
        };
        fetch();
        return () => { cancelled = true; };
    }, [projectId]);

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

    // Invite handler
    const handleInvite = useCallback(async (userId: number) => {
        setInvitingIds(prev => new Set(prev).add(userId));
        try {
            await requestService.createRequest({
                receiverId: userId,
                projectId,
                requestType: ProjectRequestType.Invitation,
            });
            setInvitedIds(prev => new Set(prev).add(userId));
        } catch {
            // Could show an error, but keep it simple for now
        } finally {
            setInvitingIds(prev => {
                const next = new Set(prev);
                next.delete(userId);
                return next;
            });
        }
    }, [projectId]);

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
            <div
                ref={panelRef}
                className="w-full max-w-lg mx-4 max-h-[80vh] bg-[#0f1f3d] border border-white/10 rounded-2xl shadow-2xl overflow-hidden flex flex-col"
            >
                {/* Header */}
                <div className="flex items-center justify-between px-6 py-4 border-b border-white/[0.06] shrink-0">
                    <div className="min-w-0">
                        <h2 className="text-lg font-bold text-slate-200 m-0 truncate">Recommended Specialists</h2>
                        <p className="text-xs text-slate-500 m-0 mt-0.5 truncate">for {projectName}</p>
                    </div>
                    <button
                        onClick={onClose}
                        className="w-8 h-8 flex items-center justify-center rounded-lg text-slate-400 hover:text-white hover:bg-white/10 transition-colors cursor-pointer shrink-0"
                    >
                        ✕
                    </button>
                </div>

                {/* Body */}
                <div className="flex-1 overflow-y-auto px-6 py-4">
                    {isLoading && (
                        <div className="flex items-center justify-center py-12">
                            <div className="w-7 h-7 border-2 border-blue-500/30 border-t-blue-400 rounded-full animate-spin" />
                        </div>
                    )}

                    {error && (
                        <div className="flex flex-col items-center gap-3 py-8 text-center">
                            <p className="text-sm text-red-400 m-0">{error}</p>
                            <button
                                onClick={() => window.location.reload()}
                                className="px-4 py-2 text-xs font-semibold text-blue-400 bg-blue-500/10 border border-blue-500/20 rounded-lg hover:bg-blue-500/20 transition-colors cursor-pointer"
                            >
                                Retry
                            </button>
                        </div>
                    )}

                    {!isLoading && !error && specialists.length === 0 && (
                        <div className="flex items-center justify-center py-12">
                            <p className="text-sm text-slate-500 m-0">No specialist matches found for this project yet.</p>
                        </div>
                    )}

                    {!isLoading && !error && specialists.length > 0 && (
                        <div className="flex flex-col gap-2">
                            {specialists.map(match => (
                                <MatchedSpecialistCard
                                    key={match.user.id}
                                    match={match}
                                    onInvite={handleInvite}
                                    isInviting={invitingIds.has(match.user.id)}
                                    isInvited={invitedIds.has(match.user.id)}
                                />
                            ))}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}
