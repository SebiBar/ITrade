import { useState } from 'react';
import { requestService } from '../../api';
import type { RequestResponse } from '../../types';

interface RequestCardProps {
    request: RequestResponse;
    /** "invitation" for specialist, "application" for client */
    variant: 'invitation' | 'application';
    /** Called after resolving so parent can refresh */
    onResolved?: () => void;
}

/** Card displaying a pending invitation or application with accept/reject actions */
export default function RequestCard({ request, variant, onResolved }: RequestCardProps) {
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

    const formatDate = (iso: string) =>
        new Date(iso).toLocaleDateString(undefined, {
            month: 'short',
            day: 'numeric',
            year: 'numeric',
        });

    const label = variant === 'invitation'
        ? `${request.senderUsername} invited you to`
        : `${request.senderUsername} applied to`;

    return (
        <div className="flex items-center justify-between gap-4 px-5 py-4 bg-white/[0.03] border border-white/[0.06] rounded-xl hover:bg-white/[0.05] transition-colors">
            <div className="flex flex-col gap-1 min-w-0">
                <p className="text-sm text-slate-300 m-0 truncate">
                    <span className="text-slate-500">{label}</span>{' '}
                    <span className="font-semibold text-slate-200">{request.projectName}</span>
                </p>
                {request.message && (
                    <p className="text-xs text-slate-500 m-0 truncate">"{request.message}"</p>
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

            <div className="flex items-center gap-2 shrink-0">
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
            </div>
        </div>
    );
}
