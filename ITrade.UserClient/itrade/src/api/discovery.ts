import apiClient from './apiClient';
import type { SearchRequest } from '../types/requests';
import type {
    ProjectMatchedResponse,
    UserMatchedResponse,
    SearchResponse,
} from '../types/responses';

export const discoveryService = {
    /**
     * Get recommended projects for a specialist
     * (Specialist role only)
     */
    async getRecommendedProjectsForSpecialist(): Promise<
        ProjectMatchedResponse[]
    > {
        const response = await apiClient.get<ProjectMatchedResponse[]>(
            '/discovery'
        );
        return response.data;
    },

    /**
     * Get recommended specialists for a specific project
     * (Client role only)
     */
    async getRecommendedSpecialistsForProject(
        projectId: number
    ): Promise<UserMatchedResponse[]> {
        const response = await apiClient.get<UserMatchedResponse[]>(
            `/discovery/projects/${projectId}`
        );
        return response.data;
    },

    /**
     * Search for projects and/or users
     */
    async search(params: SearchRequest): Promise<SearchResponse> {
        const response = await apiClient.get<SearchResponse>('/discovery/search', {
            params,
        });
        return response.data;
    },
};

export default discoveryService;
