import apiClient from './apiClient';
import type {
    ReviewCreateRequest,
    ReviewUpdateRequest,
} from '../types/requests';
import type { ReviewResponse } from '../types/responses';

export const reviewService = {
    /**
     * Get all reviews sent by the current user
     */
    async getSentReviews(): Promise<ReviewResponse[]> {
        const response = await apiClient.get<ReviewResponse[]>('/reviews');
        return response.data;
    },

    /**
     * Create a new review
     */
    async createReview(data: ReviewCreateRequest): Promise<number> {
        const response = await apiClient.post<number>('/reviews', data);
        return response.data;
    },

    /**
     * Update an existing review
     */
    async updateReview(data: ReviewUpdateRequest): Promise<void> {
        await apiClient.put('/reviews', data);
    },

    /**
     * Delete a review
     */
    async deleteReview(reviewId: number): Promise<void> {
        await apiClient.delete('/reviews', {
            params: { reviewId },
        });
    },
};

export default reviewService;
