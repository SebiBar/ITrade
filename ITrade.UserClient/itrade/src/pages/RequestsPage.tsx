import { useState, useEffect, useCallback } from 'react';
import { requestService } from '../api';
import { useUser } from '../context';
import { DashboardSection } from '../components/dashboard';
import { ReceivedRequestCard, SentRequestCard } from '../components/requests';
import type { RequestResponse } from '../types';

export default function RequestsPage() {
    const { currentUser } = useUser();
    const isClient = currentUser?.role === 'Client';

    const [invitations, setInvitations] = useState<RequestResponse[]>([]);
    const [applications, setApplications] = useState<RequestResponse[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const fetchRequests = useCallback(async () => {
        setIsLoading(true);
        setError(null);
        try {
            const data = await requestService.getUserRequests();
            setInvitations(data.invitations);
            setApplications(data.applications);
        } catch {
            setError('Failed to load requests.');
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchRequests();
    }, [fetchRequests]);

    // ── Categorise into sent vs received ─────────────────────────────────
    // Client: sent invitations, received applications
    // Specialist: received invitations, sent applications

    const receivedRequests = isClient ? applications : invitations;
    const sentRequests = isClient ? invitations : applications;

    const pendingReceivedCount = receivedRequests.filter(
        r => r.accepted === undefined || r.accepted === null,
    ).length;

    // ── Render ───────────────────────────────────────────────────────────

    const subtitle = isClient
        ? 'Manage invitations you sent and applications from specialists.'
        : 'Manage invitations from clients and applications you submitted.';

    return (
        <div className="min-h-[calc(100vh-3.5rem)] bg-[linear-gradient(135deg,#020817_0%,#0a1628_45%,#0f2154_100%)]">
            <div className="max-w-6xl mx-auto px-6 py-8">
                {/* Header */}
                <div className="flex flex-col gap-1 mb-8">
                    <h1 className="text-2xl font-bold text-slate-200 m-0">Requests</h1>
                    <p className="text-sm text-slate-500 m-0">{subtitle}</p>
                </div>

                {/* Loading */}
                {isLoading && (
                    <div className="flex items-center justify-center min-h-[40vh]">
                        <div className="flex flex-col items-center gap-3">
                            <div className="w-8 h-8 border-2 border-blue-500/30 border-t-blue-400 rounded-full animate-spin" />
                            <span className="text-sm text-slate-500">Loading requests…</span>
                        </div>
                    </div>
                )}

                {/* Error */}
                {error && !isLoading && (
                    <div className="flex items-center justify-center min-h-[40vh]">
                        <div className="flex flex-col items-center gap-3 text-center">
                            <p className="text-sm text-red-400 m-0">{error}</p>
                            <button
                                onClick={fetchRequests}
                                className="px-4 py-2 text-xs font-semibold text-blue-400 bg-blue-500/10 border border-blue-500/20 rounded-lg hover:bg-blue-500/20 transition-colors cursor-pointer"
                            >
                                Retry
                            </button>
                        </div>
                    </div>
                )}

                {/* Content */}
                {!isLoading && !error && (
                    <div className="flex flex-col gap-10">
                        {/* ── Received requests ──────────────────────────── */}
                        <DashboardSection
                            title={isClient ? 'Received Applications' : 'Received Invitations'}
                            badge={pendingReceivedCount}
                            subtitle={
                                isClient
                                    ? 'Specialists who want to work on your projects.'
                                    : 'Clients who want you on their projects.'
                            }
                        >
                            {receivedRequests.length === 0 ? (
                                <EmptyState message="No received requests to show." />
                            ) : (
                                <div className="flex flex-col gap-3">
                                    {receivedRequests.map(r => (
                                        <ReceivedRequestCard
                                            key={r.id}
                                            request={r}
                                            onResolved={fetchRequests}
                                        />
                                    ))}
                                </div>
                            )}
                        </DashboardSection>

                        {/* ── Sent requests ──────────────────────────────── */}
                        <DashboardSection
                            title={isClient ? 'Sent Invitations' : 'Sent Applications'}
                            badge={sentRequests.length}
                            subtitle={
                                isClient
                                    ? 'Invitations you sent to specialists.'
                                    : 'Applications you submitted to projects.'
                            }
                        >
                            {sentRequests.length === 0 ? (
                                <EmptyState message="No sent requests to show." />
                            ) : (
                                <div className="flex flex-col gap-3">
                                    {sentRequests.map(r => (
                                        <SentRequestCard
                                            key={r.id}
                                            request={r}
                                            onDeleted={fetchRequests}
                                        />
                                    ))}
                                </div>
                            )}
                        </DashboardSection>
                    </div>
                )}
            </div>
        </div>
    );
}

/** Small reusable empty-state placeholder */
function EmptyState({ message }: { message: string }) {
    return (
        <div className="flex items-center justify-center py-12 px-6 bg-white/[0.02] border border-dashed border-white/[0.06] rounded-xl">
            <p className="text-sm text-slate-500 m-0">{message}</p>
        </div>
    );
}
