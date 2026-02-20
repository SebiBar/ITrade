import {
    createContext,
    useContext,
    useEffect,
    useState,
    type ReactNode,
} from 'react';
import { authService } from '../api/auth';
import { userService } from '../api/user';
import { TokenManager } from '../api/apiClient';
import type { UserResponse } from '../types/responses';
import type { LoginRequest, RegisterRequest } from '../types/requests';

// ─── Types ────────────────────────────────────────────────────────────────────

interface UserContextValue {
    /** Basic info (id, username, role) of the currently authenticated user, or null if not logged in */
    currentUser: UserResponse | null;
    /** True while the initial auth check or an auth action is in progress */
    isLoading: boolean;
    /** Non-null when an auth action fails */
    authError: string | null;

    login: (data: LoginRequest) => Promise<void>;
    register: (data: RegisterRequest) => Promise<void>;
    logout: () => Promise<void>;
    clearAuthError: () => void;
}

// ─── Context ──────────────────────────────────────────────────────────────────

const UserContext = createContext<UserContextValue | undefined>(undefined);

// ─── Provider ─────────────────────────────────────────────────────────────────

export function UserProvider({ children }: { children: ReactNode }) {
    const [currentUser, setCurrentUser] = useState<UserResponse | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [authError, setAuthError] = useState<string | null>(null);

    // On mount: restore session from stored JWT
    useEffect(() => {
        const controller = new AbortController();

        const restore = async () => {
            const token = TokenManager.getJwtToken();
            if (!token) {
                setIsLoading(false);
                return;
            }
            try {
                const profile = await userService.getCurrentUserProfile();
                if (!controller.signal.aborted) setCurrentUser(profile.user);
            } catch {
                // Token is expired / invalid – clear it silently
                if (!controller.signal.aborted) TokenManager.clearTokens();
            } finally {
                if (!controller.signal.aborted) setIsLoading(false);
            }
        };

        restore();
        return () => controller.abort();
    }, []);

    // ── Actions ────────────────────────────────────────────────────────────────

    const login = async (data: LoginRequest) => {
        setIsLoading(true);
        setAuthError(null);
        try {
            const response = await authService.login(data);
            setCurrentUser(response.user);
        } catch (err: unknown) {
            const message =
                err instanceof Error ? err.message : 'Login failed. Please try again.';
            setAuthError(message);
            throw err;
        } finally {
            setIsLoading(false);
        }
    };

    const register = async (data: RegisterRequest) => {
        setIsLoading(true);
        setAuthError(null);
        try {
            await authService.register(data);
        } catch (err: unknown) {
            const message =
                err instanceof Error
                    ? err.message
                    : 'Registration failed. Please try again.';
            setAuthError(message);
            throw err;
        } finally {
            setIsLoading(false);
        }
    };

    const logout = async () => {
        try {
            await authService.logout();
        } finally {
            setCurrentUser(null);
            setAuthError(null);
        }
    };

    const clearAuthError = () => setAuthError(null);

    return (
        <UserContext.Provider
            value={{ currentUser, isLoading, authError, login, register, logout, clearAuthError }}
        >
            {children}
        </UserContext.Provider>
    );
}

// ─── Hook ─────────────────────────────────────────────────────────────────────

export function useUser(): UserContextValue {
    const ctx = useContext(UserContext);
    if (!ctx) throw new Error('useUser must be used inside <UserProvider>');
    return ctx;
}

export default UserContext;
