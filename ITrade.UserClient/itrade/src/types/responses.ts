// Base Models
export interface UserResponse {
    id: number;
    username: string;
    role: string;
}

export interface Tag {
    id: number;
    name: string;
}

export interface UserProfileLinkResponse {
    id: number;
    userId: number;
    url: string;
}

export interface UserProfileTagResponse {
    id: number;
    name: string;
}

export interface ProjectTagResponse {
    id: number;
    name: string;
}

export interface ProjectResponse {
    id: number;
    name: string;
    description?: string;
    ownerId: number;
    ownerUsername: string;
    workerId?: number;
    workerUsername?: string;
    deadline: string;
    projectStatusTypeId: number;
    projectStatusType: string;
    tags: ProjectTagResponse[];
    createdAt: string;
    updatedAt: string;
}

export interface ReviewResponse {
    id: number;
    reviewerId: number;
    reviewerUsername: string;
    revieweeId: number;
    revieweeUsername: string;
    rating: number;
    title?: string;
    comment?: string;
    createdAt: string;
}

export interface RequestResponse {
    id: number;
    message?: string;
    senderId: number;
    senderUsername: string;
    receiverId: number;
    receiverUsername: string;
    projectId: number;
    projectName: string;
    requestType: string;
    accepted?: boolean;
    createdAt: string;
    matchScore?: number;
}

export interface NotificationResponse {
    id: number;
    name: string;
    content: string;
    isRead: boolean;
    createdAt: string;
}

// Auth Responses
export interface LoginResponse {
    user: UserResponse;
    jwt: string;
    refresh: string;
}

export interface RefreshTokensResponse {
    jwt: string;
    refresh: string;
}

// User Responses
export interface CurrentUserResponse {
    user: UserResponse;
    email: string;
    createdAt: string;
    updatedAt: string;
    isEmailConfirmed: boolean;
    profileLinks: UserProfileLinkResponse[];
    profileTags: UserProfileTagResponse[];
    projects: ProjectResponse[];
    reviews: ReviewResponse[];
}

export interface UserProfileResponse {
    user: UserResponse;
    profileLinks: UserProfileLinkResponse[];
    profileTags: UserProfileTagResponse[];
    projects: ProjectResponse[];
    reviews: ReviewResponse[];
}

// Request Responses
export interface UserRequestsResponse {
    invitations: RequestResponse[];
    applications: RequestResponse[];
}

// Search Response
export interface SearchResponse {
    projects: ProjectResponse[];
    users: UserResponse[];
    totalProjects: number;
    totalUsers: number;
    page: number;
    pageSize: number;
}

// Dashboard Responses
export interface DashboardResponse {
    unreadNotificationCount: number;
}

export interface UserMatchedResponse {
    user: UserResponse;
    matchPercentage: number;
}

export interface ProjectWithMatchesResponse {
    project: ProjectResponse;
    recommendedSpecialists: UserMatchedResponse[];
}

export interface ProjectMatchedResponse {
    project: ProjectResponse;
    matchPercentage: number;
}

export interface DashboardClientResponse extends DashboardResponse {
    pendingApplications: RequestResponse[];
    openProjectsWithMatches: ProjectWithMatchesResponse[];
    activeProjectCount: number;
}

export interface DashboardSpecialistResponse extends DashboardResponse {
    pendingInvitations: RequestResponse[];
    activeProjects: ProjectResponse[];
    recommendedProjects: ProjectMatchedResponse[];
}

export interface DashboardAdminResponse extends DashboardResponse {
    tags: Tag[];
}

// Union type for all dashboard responses
export type AnyDashboardResponse =
    | DashboardClientResponse
    | DashboardSpecialistResponse
    | DashboardAdminResponse;
