import apiClient, { TokenManager } from './apiClient';
import type {
    LoginRequest,
    RegisterRequest,
    ResolveForgotPasswordRequest,
} from '../types/requests';
import type { LoginResponse, RefreshTokensResponse } from '../types/responses';

export const authService = {
    /**
     * Register a new user
     */
    async register(data: RegisterRequest): Promise<void> {
        await apiClient.post('/auth/register', data);
    },

    /**
     * Verify email with token from email
     */
    async verifyEmail(token: string): Promise<void> {
        await apiClient.post('/auth/verify-email', null, {
            params: { token },
        });
    },

    /**
     * Request password reset email
     */
    async forgotPassword(email: string): Promise<void> {
        await apiClient.post('/auth/forgot-password', null, {
            params: { email },
        });
    },

    /**
     * Reset password with token from email
     */
    async resolveForgotPassword(
        data: ResolveForgotPasswordRequest
    ): Promise<void> {
        await apiClient.post('/auth/resolve-forgot-password', data);
    },

    /**
     * Change password (requires authentication)
     */
    async changePassword(newPassword: string): Promise<void> {
        await apiClient.post('/auth/change-password', null, {
            params: { newPassword },
        });
    },

    /**
     * Login with email and password
     */
    async login(data: LoginRequest): Promise<LoginResponse> {
        const response = await apiClient.post<LoginResponse>('/auth/login', data);
        // Store tokens
        TokenManager.setTokens(response.data.jwt, response.data.refresh);
        return response.data;
    },

    /**
     * Refresh access token
     */
    async refreshTokens(refreshToken: string): Promise<RefreshTokensResponse> {
        const response = await apiClient.post<RefreshTokensResponse>(
            '/auth/refresh-tokens',
            null,
            {
                params: { refreshToken },
            }
        );
        // Update stored tokens
        TokenManager.setTokens(response.data.jwt, response.data.refresh);
        return response.data;
    },

    /**
     * Logout (clear local tokens)
     */
    logout(): void {
        TokenManager.clearTokens();
    },
};

export default authService;
