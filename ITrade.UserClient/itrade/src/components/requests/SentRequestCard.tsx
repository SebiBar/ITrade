import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { requestService } from '../../api';
import type { RequestResponse } from '../../types';

interface SentRequestCardProps {
    request: RequestResponse;
    onDeleted?: () => void;
}

/** Card for a sent request (invitation or application) with a Delete action */
export default function SentRequestCard({ request, onDeleted }: SentRequestCardProps) {
    const navigate = useNavigate();
    const [isDeleting, setIsDeleting] = useState(false);
    const [confirmDelete, setConfirmDelete] = useState(false);

    const handleDelete = async () => {
        setIsDeleting(true);
        try {
            await requestService.deleteRequest(request.id);
            onDeleted?.();
        } catch {
            // silently fail
        } finally {
            setIsDeleting(false);
            setConfirmDelete(false);
        }
    };

    const isResolved = request.accepted !== undefined && request.accepted !== null;
    const wasAccepted = request.accepted === true;

    const label =
        request.requestType === 'Invitation'
            ? `You invited ${request.receiverUsername} to`
            : `You applied to`;

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
                ) : confirmDelete ? (
                    <>
                        <button
                            onClick={handleDelete}
                            disabled={isDeleting}
                            className="px-3.5 py-1.5 text-xs font-semibold rounded-lg bg-red-500/15 text-red-400 border border-red-500/20 hover:bg-red-500/25 transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            {isDeleting ? 'Deleting…' : 'Confirm'}
                        </button>
                        <button
                            onClick={() => setConfirmDelete(false)}
                            disabled={isDeleting}
                            className="px-3.5 py-1.5 text-xs font-semibold rounded-lg bg-white/[0.05] text-slate-400 border border-white/[0.08] hover:bg-white/[0.08] transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            Cancel
                        </button>
                    </>
                ) : (
                    <button
                        onClick={() => setConfirmDelete(true)}
                        className="px-3.5 py-1.5 text-xs font-semibold rounded-lg bg-red-500/10 text-red-400 border border-red-500/20 hover:bg-red-500/20 transition-colors cursor-pointer"
                    >
                        Delete
                    </button>
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
