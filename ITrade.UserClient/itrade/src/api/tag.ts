import apiClient from './apiClient';
import type { Tag } from '../types/responses';

export const tagService = {
    /**
     * Search for tags by query string
     */
    async searchTags(query: string): Promise<Tag[]> {
        const response = await apiClient.get<Tag[]>('/tags/search', {
            params: { query },
        });
        return response.data;
    },

    /**
     * Get all tags (Admin only)
     */
    async getAllTags(): Promise<Tag[]> {
        const response = await apiClient.get<Tag[]>('/tags');
        return response.data;
    },

    /**
     * Create a new tag (Admin only)
     */
    async createTag(tagName: string): Promise<Tag> {
        const response = await apiClient.post<Tag>('/tags', JSON.stringify(tagName), {
            headers: {
                'Content-Type': 'application/json',
            },
        });
        return response.data;
    },

    /**
     * Delete a tag (Admin only)
     */
    async deleteTag(tagId: number): Promise<void> {
        await apiClient.delete(`/tags/${tagId}`);
    },
};

export default tagService;
