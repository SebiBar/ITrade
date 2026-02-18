// Export API client and token manager
export { apiClient, TokenManager } from './apiClient';

// Export all services
export { default as authService } from './auth';
export { default as userService } from './user';
export { default as projectService } from './project';
export { default as requestService } from './request';
export { default as notificationService } from './notification';
export { default as reviewService } from './review';
export { default as dashboardService } from './dashboard';
export { default as discoveryService } from './discovery';
export { default as tagService } from './tag';

// Export all types
export * from '../types';
