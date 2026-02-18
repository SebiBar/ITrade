import type {
    UserRole,
    ProjectStatusType,
    ProjectRequestType,
    SearchEntityType,
    SearchSortBy,
    SortDirection,
} from './enums';

// Auth Requests
export interface LoginRequest {
    email: string;
    password: string;
}

export interface RegisterRequest {
    username: string;
    email: string;
    password: string;
    role: UserRole;
}

export interface ResolveForgotPasswordRequest {
    emailedToken: string;
    newPassword: string;
}

// Project Requests
export interface ProjectRequest {
    name: string;
    description?: string;
    deadline: string; // ISO date string
    status?: ProjectStatusType;
    tagIds: number[];
}

export interface ProjectUpdateRequest {
    name?: string;
    description?: string;
    deadline?: string; // ISO date string
    status?: ProjectStatusType;
}

// Request Requests
export interface RequestReq {
    message?: string;
    receiverId: number;
    projectId: number;
    requestType: ProjectRequestType;
}

// Review Requests
export interface ReviewCreateRequest {
    revieweeId: number;
    title: string;
    comment?: string;
    rating: number;
}

export interface ReviewUpdateRequest {
    reviewId: number;
    title?: string;
    comment?: string;
    rating?: number;
}

// Search Request
export interface SearchRequest {
    query?: string;
    entityType?: SearchEntityType;
    tagIds?: number[];
    userRoleId?: number;
    projectStatusId?: number;
    deadlineFrom?: string; // ISO date string
    deadlineTo?: string; // ISO date string
    createdFrom?: string; // ISO date string
    createdTo?: string; // ISO date string
    sortBy?: SearchSortBy;
    sortDirection?: SortDirection;
    page?: number;
    pageSize?: number;
}

// Notification Request
export interface NotificationRequest {
    name: string;
    content: string;
    userId: number;
}
