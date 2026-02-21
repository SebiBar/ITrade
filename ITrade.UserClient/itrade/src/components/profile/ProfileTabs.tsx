export type ProfileTab = 'projects' | 'reviews';

interface ProfileTabsProps {
    activeTab: ProfileTab;
    onTabChange: (tab: ProfileTab) => void;
    projectCount: number;
    reviewCount: number;
}

/** Tab bar to switch between Projects and Reviews */
export default function ProfileTabs({
    activeTab,
    onTabChange,
    projectCount,
    reviewCount,
}: ProfileTabsProps) {
    const tabs: { key: ProfileTab; label: string; count: number }[] = [
        { key: 'projects', label: 'Projects', count: projectCount },
        { key: 'reviews', label: 'Reviews', count: reviewCount },
    ];

    return (
        <div className="flex gap-1 p-1 bg-white/[0.03] border border-white/[0.06] rounded-xl">
            {tabs.map(tab => (
                <button
                    key={tab.key}
                    onClick={() => onTabChange(tab.key)}
                    className={`flex-1 px-4 py-2 text-xs font-semibold rounded-lg border transition-all cursor-pointer ${activeTab === tab.key
                            ? 'bg-blue-500/15 text-blue-400 border-blue-500/20'
                            : 'bg-transparent text-slate-500 border-transparent hover:text-slate-300'
                        }`}
                >
                    {tab.label}
                    <span className="ml-1.5 text-[0.6rem] opacity-70">({tab.count})</span>
                </button>
            ))}
        </div>
    );
}
