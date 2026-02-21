import apiClient from './apiClient';
import type { RequestReq } from '../types/requests';
import type { UserRequestsResponse, RequestResponse } from '../types/responses';

export const requestService = {
    /**
     * Get all requests for the current user (invitations and applications)
     */
    async getUserRequests(): Promise<UserRequestsResponse> {
        const response = await apiClient.get<UserRequestsResponse>('/requests');
        return response.data;
    },

    /**
     * Create a new request (invitation or application)
     */
    async createRequest(data: RequestReq): Promise<RequestResponse> {
        const response = await apiClient.post<RequestResponse>('/requests', data);
        return response.data;
    },

    /**
     * Check if user already applied to project
     */
    async hasApplied(projectId: number): Promise<boolean> {
        const response = await apiClient.get<boolean>(
            `/requests/has-applied/${projectId}`);
        return response.data;
    },

    /**
     * Resolve a request (accept or reject)
     */
    async resolveRequest(requestId: number, accepted: boolean): Promise<void> {
        await apiClient.post(`/requests/${requestId}`, null, {
            params: { accepted },
        });
    },

    /**
     * Delete a request
     */
    async deleteRequest(requestId: number): Promise<void> {
        await apiClient.delete(`/requests/${requestId}`);
    },
};

export default requestService;
