import { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import type { UserResponse } from '../../types';

interface UserMenuProps {
    user: UserResponse;
    onLogout: () => void;
}

const MENU_ITEMS = [
    { label: 'Profile', path: '/profile' },
    { label: 'My Projects', path: '/my-projects' },
    { label: 'Requests', path: '/requests' },
    { label: 'Settings', path: '/settings' },
] as const;

export default function UserMenu({ user, onLogout }: UserMenuProps) {
    const [isOpen, setIsOpen] = useState(false);
    const navigate = useNavigate();
    const ref = useRef<HTMLDivElement>(null);

    const initials = user.username.slice(0, 2).toUpperCase();

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

    const handleNavigate = (path: string) => {
        navigate(path);
        setIsOpen(false);
    };

    const handleLogout = () => {
        setIsOpen(false);
        onLogout();
    };

    return (
        <div ref={ref} className="relative">
            {/* Avatar button */}
            <button
                onClick={() => setIsOpen(prev => !prev)}
                aria-label="User menu"
                className="w-9 h-9 rounded-full bg-gradient-to-br from-blue-500 to-indigo-600 flex items-center justify-center text-white font-bold text-xs cursor-pointer hover:opacity-90 active:scale-95 transition-all select-none shadow-[0_2px_10px_rgba(79,125,255,0.35)]"
            >
                {initials}
            </button>

            {/* Dropdown */}
            {isOpen && (
                <div className="absolute right-0 mt-2 w-52 bg-[#0f1f3d] border border-white/10 rounded-xl shadow-2xl z-50 overflow-hidden">
                    {/* User info header */}
                    <div className="px-4 py-3 border-b border-white/10">
                        <p className="text-white font-semibold text-sm truncate">{user.username}</p>
                        <p className="text-slate-400 text-xs capitalize mt-0.5">{user.role}</p>
                    </div>

                    {/* Nav items */}
                    <div className="py-1">
                        {MENU_ITEMS.map(({ label, path }) => (
                            <button
                                key={label}
                                onClick={() => handleNavigate(path)}
                                className="w-full text-left px-4 py-2.5 text-slate-300 hover:bg-white/5 hover:text-white text-sm transition-colors"
                            >
                                {label}
                            </button>
                        ))}
                    </div>

                    {/* Logout */}
                    <div className="border-t border-white/10 py-1">
                        <button
                            onClick={handleLogout}
                            className="w-full text-left px-4 py-2.5 text-red-400 hover:bg-white/5 hover:text-red-300 text-sm transition-colors"
                        >
                            Log out
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}
