import { useUser } from '../context';
import { UserRole } from '../types';
import { SpecialistDashboard, ClientDashboard } from '../components/dashboard';

export default function DashboardPage() {
    const { currentUser } = useUser();

    const roleId = currentUser?.role === 'Client'
        ? UserRole.Client
        : currentUser?.role === 'Specialist'
            ? UserRole.Specialist
            : null;

    return (
        <div className="min-h-[calc(100vh-3.5rem)] bg-[linear-gradient(135deg,#020817_0%,#0a1628_45%,#0f2154_100%)]">
            <div className="max-w-6xl mx-auto px-6 py-8">
                {/* Welcome header */}
                <div className="flex flex-col gap-1 mb-8">
                    <h1 className="text-2xl font-bold text-slate-200 m-0">
                        Welcome back,{' '}
                        <span className="bg-gradient-to-br from-blue-400 to-indigo-400 bg-clip-text text-transparent">
                            {currentUser?.username ?? 'User'}
                        </span>
                    </h1>
                    <p className="text-sm text-slate-500 m-0">
                        Here's what's happening across your projects today.
                    </p>
                </div>

                {/* Role-specific dashboard */}
                {roleId === UserRole.Specialist && <SpecialistDashboard />}
                {roleId === UserRole.Client && <ClientDashboard />}
                {roleId === null && (
                    <p className="text-sm text-slate-500">Unknown role.</p>
                )}
            </div>
        </div>
    );
}
