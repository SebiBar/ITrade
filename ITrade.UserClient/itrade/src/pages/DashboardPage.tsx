import { useUser } from '../context';

export default function DashboardPage() {
    const { currentUser } = useUser();

    return (
        <div className="min-h-[calc(100vh-3.5rem)] flex flex-col items-center justify-center gap-4 bg-[linear-gradient(135deg,#020817_0%,#0a1628_45%,#0f2154_100%)]">
            <h1 className="text-4xl font-bold text-slate-200 m-0">
                Welcome,{' '}
                <span className="bg-gradient-to-br from-blue-400 to-indigo-400 bg-clip-text text-transparent">
                    {currentUser?.username ?? 'User'}
                </span>
            </h1>
            <p className="text-slate-500 m-0 text-sm">
                Role: {currentUser?.role}
            </p>
        </div>
    );
}
