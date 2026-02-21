import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { requestService } from '../../api';
import type { RequestResponse } from '../../types';

interface ReceivedRequestCardProps {
    request: RequestResponse;
    onResolved?: () => void;
}

/** Card for a received request (invitation or application) with Accept / Decline actions */
export default function ReceivedRequestCard({ request, onResolved }: ReceivedRequestCardProps) {
    const navigate = useNavigate();
    const [isResolving, setIsResolving] = useState(false);

    const handleResolve = async (accepted: boolean) => {
        setIsResolving(true);
        try {
            await requestService.resolveRequest(request.id, accepted);
            onResolved?.();
        } catch {
            // silently fail — parent will still show stale data
        } finally {
            setIsResolving(false);
        }
    };

    const isResolved = request.accepted !== undefined && request.accepted !== null;
    const wasAccepted = request.accepted === true;

    const label =
        request.requestType === 'Invitation'
            ? `${request.senderUsername} invited you to`
            : `${request.senderUsername} applied to`;

    return (
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 sm:gap-4 px-4 sm:px-5 py-4 bg-white/[0.03] border border-white/[0.06] rounded-xl hover:bg-white/[0.05] transition-colors">
            <div className="flex flex-col gap-1 min-w-0">
                <p className="text-sm text-slate-300 m-0 sm:truncate">
                    <span className="text-slate-500">{label}</span>{' '}
                    <button
                        onClick={() => navigate(`/projects/${request.projectId}`)}
                        className="font-semibold text-slate-200 hover:text-blue-400 bg-transparent border-none p-0 cursor-pointer transition-colors text-left"
                    >
                        {request.projectName}
                    </button>
                </p>
                {request.message && (
                    <p className="text-xs text-slate-500 m-0 sm:truncate italic">"{request.message}"</p>
                )}
                <div className="flex items-center gap-2">
                    <p className="text-[0.65rem] text-slate-600 m-0">{formatDate(request.createdAt)}</p>
                    {request.matchScore != null && (
                        <span className="text-[0.65rem] font-medium text-indigo-400">
                            {Math.round(request.matchScore)}% match
                        </span>
                    )}
                </div>
            </div>

            <div className="flex items-center gap-2 shrink-0 self-start sm:self-center">
                {isResolved ? (
                    <span
                        className={`px-3 py-1.5 text-xs font-semibold rounded-lg ${wasAccepted
                            ? 'bg-emerald-500/15 text-emerald-400 border border-emerald-500/20'
                            : 'bg-red-500/10 text-red-400 border border-red-500/20'
                            }`}
                    >
                        {wasAccepted ? 'Accepted' : 'Declined'}
                    </span>
                ) : (
                    <>
                        <button
                            onClick={() => handleResolve(true)}
                            disabled={isResolving}
                            className="px-3.5 py-1.5 text-xs font-semibold rounded-lg bg-emerald-500/15 text-emerald-400 border border-emerald-500/20 hover:bg-emerald-500/25 transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            Accept
                        </button>
                        <button
                            onClick={() => handleResolve(false)}
                            disabled={isResolving}
                            className="px-3.5 py-1.5 text-xs font-semibold rounded-lg bg-red-500/10 text-red-400 border border-red-500/20 hover:bg-red-500/20 transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            Decline
                        </button>
                    </>
                )}
            </div>
        </div>
    );
}

function formatDate(iso: string) {
    return new Date(iso).toLocaleDateString(undefined, {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
    });
}
