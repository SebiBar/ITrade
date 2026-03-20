import apiClient from './apiClient';
import type {
    CurrentUserResponse,
    UserProfileResponse,
    UserProfileTagResponse,
    UserProfileLinkResponse,
} from '../types/responses';

export const userService = {
    /**
     * Get current user profile (requires authentication)
     */
    async getCurrentUserProfile(): Promise<CurrentUserResponse> {
        const response = await apiClient.get<CurrentUserResponse>('/user/me');
        return response.data;
    },

    /**
     * Get user profile by ID
     */
    async getUserProfile(userId: number): Promise<UserProfileResponse> {
        const response = await apiClient.get<UserProfileResponse>(
            `/user/${userId}`
        );
        return response.data;
    },

    /**
     * Change username
     */
    async changeUsername(newUsername: string): Promise<void> {
        await apiClient.put('/user', null, {
            params: { newUsername },
        });
    },

    /**
     * Update matching preferences
     */
    async updateMatchingPreferences(preference: number): Promise<void> {
        await apiClient.put('/user/matching-preferences', null, {
            params: { preference },
        });
    },

    /**
     * Add a profile tag (Specialist/Admin only)
     */
    async addProfileTag(tagId: number): Promise<UserProfileTagResponse> {
        const response = await apiClient.post<UserProfileTagResponse>(
            '/user/tags',
            null,
            {
                params: { tagId },
            }
        );
        return response.data;
    },

    /**
     * Remove a profile tag (Specialist/Admin only)
     */
    async removeProfileTag(tagId: number): Promise<void> {
        await apiClient.delete(`/user/tags/${tagId}`);
    },

    /**
     * Create a profile link
     */
    async createProfileLink(url: string): Promise<UserProfileLinkResponse> {
        const response = await apiClient.post<UserProfileLinkResponse>(
            '/user/links',
            null,
            {
                params: { url },
            }
        );
        return response.data;
    },

    /**
     * Remove a profile link
     */
    async removeProfileLink(profileLinkId: number): Promise<void> {
        await apiClient.delete(`/user/links/${profileLinkId}`);
    },

    /**
     * Soft delete current user account
     */
    async softDeleteAccount(): Promise<void> {
        await apiClient.delete('/user/me');
    },

    /**
     * Hard delete a user (Admin only)
     */
    async hardDeleteUser(userId: number): Promise<void> {
        await apiClient.delete(`/user/${userId}`);
    },

    /**
     * Restore a soft-deleted user (Admin only)
     */
    async restoreUser(userId: number): Promise<void> {
        await apiClient.post(`/user/${userId}/restore`);
    },

    /**
     * Ping
    */
    async ping(): Promise<string> {
        const response = await apiClient.get<string>('/user/ping');
        return response.data;
    },
};

export default userService;
