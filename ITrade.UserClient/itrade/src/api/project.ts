import apiClient from './apiClient';
import type { ProjectRequest, ProjectUpdateRequest } from '../types/requests';
import type { ProjectResponse, ProjectTagResponse } from '../types/responses';

export const projectService = {
    /**
     * Get project by ID
     */
    async getProject(projectId: number): Promise<ProjectResponse> {
        const response = await apiClient.get<ProjectResponse>(
            `/projects/${projectId}`
        );
        return response.data;
    },

    /**
     * Create a new project (Client only)
     */
    async createProject(data: ProjectRequest): Promise<ProjectResponse> {
        const response = await apiClient.post<ProjectResponse>('/projects', data);
        return response.data;
    },

    /**
     * Update a project (Client only)
     */
    async updateProject(
        projectId: number,
        data: ProjectUpdateRequest
    ): Promise<void> {
        await apiClient.put(`/projects/${projectId}`, data);
    },

    /**
     * Soft delete a project (Client only)
     */
    async softDeleteProject(projectId: number): Promise<void> {
        await apiClient.delete(`/projects/${projectId}`);
    },

    /**
     * Get deleted projects (Client/Admin only)
     */
    async getDeletedProjects(): Promise<ProjectResponse[]> {
        const response = await apiClient.get<ProjectResponse[]>(
            '/projects/deleted'
        );
        return response.data;
    },

    /**
     * Hard delete a project permanently (Admin only)
     */
    async hardDeleteProject(projectId: number): Promise<void> {
        await apiClient.delete(`/projects/${projectId}/permanent`);
    },

    /**
     * Restore a soft-deleted project (Client/Admin only)
     */
    async restoreProject(projectId: number): Promise<void> {
        await apiClient.post(`/projects/${projectId}/restore`);
    },

    /**
     * Add a tag to a project (Client only)
     */
    async addProjectTag(
        projectId: number,
        tagId: number
    ): Promise<ProjectTagResponse> {
        const response = await apiClient.post<ProjectTagResponse>(
            `/projects/${projectId}/tags`,
            tagId
        );
        return response.data;
    },

    /**
     * Remove a tag from a project (Client only)
     */
    async removeProjectTag(projectId: number, tagId: number): Promise<void> {
        await apiClient.delete(`/projects/${projectId}/tags/${tagId}`);
    },
};

export default projectService;
