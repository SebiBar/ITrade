import { useState, useEffect, useRef } from 'react';
import { notificationService } from '../../api';
import type { NotificationResponse } from '../../types';

export default function NotificationsBell() {
    const [notifications, setNotifications] = useState<NotificationResponse[]>([]);
    const [isOpen, setIsOpen] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const ref = useRef<HTMLDivElement>(null);

    const unreadCount = notifications.filter(n => !n.isRead).length;

    // Fetch on mount to show initial badge count
    useEffect(() => {
        fetchNotifications();
    }, []);

    // Close dropdown on outside click
    useEffect(() => {
        const handleClickOutside = (e: MouseEvent) => {
            if (ref.current && !ref.current.contains(e.target as Node)) {
                setIsOpen(false);
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    const fetchNotifications = async () => {
        setIsLoading(true);
        try {
            const data = await notificationService.getNotifications();
            setNotifications(data);
        } catch {
            // Silently ignore — badge just won't update
        } finally {
            setIsLoading(false);
        }
    };

    const handleClick = () => {
        fetchNotifications();
        setIsOpen(prev => !prev);
    };

    const formatDate = (iso: string) =>
        new Date(iso).toLocaleDateString(undefined, {
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
        });

    return (
        <div ref={ref} className="relative">
            {/* Bell button */}
            <button
                onClick={handleClick}
                aria-label="Notifications"
                className="relative p-2 text-slate-400 hover:text-white transition-colors rounded-lg hover:bg-white/5"
            >
                <svg
                    xmlns="http://www.w3.org/2000/svg"
                    className="w-5 h-5"
                    viewBox="0 0 24 24"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth={2}
                    strokeLinecap="round"
                    strokeLinejoin="round"
                >
                    <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" />
                    <path d="M13.73 21a2 2 0 0 1-3.46 0" />
                </svg>

                {unreadCount > 0 && (
                    <span className="absolute top-1 right-1 min-w-[1rem] h-4 px-0.5 bg-blue-500 text-white text-[0.6rem] rounded-full flex items-center justify-center font-bold leading-none">
                        {unreadCount > 99 ? '99+' : unreadCount}
                    </span>
                )}
            </button>

            {/* Dropdown */}
            {isOpen && (
                <div className="fixed inset-x-0 top-14 mx-2 sm:absolute sm:inset-auto sm:right-0 sm:top-auto sm:mx-0 sm:mt-2 w-auto sm:w-80 bg-[#0f1f3d] border border-white/10 rounded-xl shadow-2xl z-50 overflow-hidden">
                    <div className="px-4 py-3 border-b border-white/10 flex items-center justify-between">
                        <h3 className="text-white font-semibold text-sm">Notifications</h3>
                        {unreadCount > 0 && (
                            <span className="text-xs text-blue-400">{unreadCount} unread</span>
                        )}
                    </div>

                    <div className="max-h-80 overflow-y-auto">
                        {isLoading ? (
                            <div className="py-8 flex items-center justify-center text-slate-500 text-sm">
                                Loading…
                            </div>
                        ) : notifications.length === 0 ? (
                            <div className="py-8 flex items-center justify-center text-slate-500 text-sm">
                                No notifications
                            </div>
                        ) : (
                            notifications.map(n => (
                                <div
                                    key={n.id}
                                    className={`px-4 py-3 border-b border-white/5 last:border-0 ${n.isRead ? 'opacity-60' : ''}`}
                                >
                                    <div className="flex items-start gap-2">
                                        {!n.isRead && (
                                            <span className="mt-1.5 w-1.5 h-1.5 rounded-full bg-blue-400 flex-shrink-0" />
                                        )}
                                        <div className={n.isRead ? '' : 'pl-0'}>
                                            <p className="text-slate-200 text-xs font-semibold">{n.name}</p>
                                            <p className="text-slate-400 text-xs mt-0.5 leading-relaxed">{n.content}</p>
                                            <p className="text-slate-600 text-[0.65rem] mt-1">{formatDate(n.createdAt)}</p>
                                        </div>
                                    </div>
                                </div>
                            ))
                        )}
                    </div>
                </div>
            )}
        </div>
    );
}
