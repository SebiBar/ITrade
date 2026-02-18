import apiClient from './apiClient';
import type { NotificationResponse } from '../types/responses';

export const notificationService = {
    /**
     * Get all notifications for the current user
     */
    async getNotifications(): Promise<NotificationResponse[]> {
        const response = await apiClient.get<NotificationResponse[]>(
            '/notifications'
        );
        return response.data;
    },

    /**
     * Delete a notification
     */
    async deleteNotification(notificationId: number): Promise<void> {
        await apiClient.delete('/notifications', {
            params: { notificationId },
        });
    },
};

export default notificationService;
