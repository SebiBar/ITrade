import { useNavigate } from 'react-router-dom';
import { useUser } from '../../context';
import NotificationsBell from './NotificationsBell';
import UserMenu from './UserMenu';

export default function Navbar() {
    const { currentUser, logout } = useUser();
    const navigate = useNavigate();

    if (!currentUser) return null;

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    return (
        <header className="fixed top-0 inset-x-0 z-40 h-14 flex items-center px-6 bg-[#020817]/90 backdrop-blur border-b border-white/[0.06] shadow-[0_1px_20px_rgba(0,0,0,0.4)]">
            {/* Brand */}
            <button
                onClick={() => navigate('/dashboard')}
                className="font-bold text-lg tracking-tight bg-gradient-to-br from-blue-400 to-indigo-400 bg-clip-text text-transparent select-none cursor-pointer hover:opacity-80 transition-opacity"
            >
                ITrade
            </button>

            {/* Spacer */}
            <div className="flex-1" />

            {/* Right-side controls */}
            <div className="flex items-center gap-1">
                <NotificationsBell />
                <UserMenu user={currentUser} onLogout={handleLogout} />
            </div>
        </header>
    );
}
