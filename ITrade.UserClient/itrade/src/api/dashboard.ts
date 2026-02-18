import apiClient from './apiClient';
import type { AnyDashboardResponse } from '../types/responses';

export const dashboardService = {
    /**
     * Get dashboard data based on user role
     * Returns different data structure based on role:
     * - Client: DashboardClientResponse
     * - Specialist: DashboardSpecialistResponse
     * - Admin: DashboardAdminResponse
     */
    async getDashboard(): Promise<AnyDashboardResponse> {
        const response = await apiClient.get<AnyDashboardResponse>('/dashboard');
        return response.data;
    },
};

export default dashboardService;
