// User Roles
export const UserRole = {
    Client: 1,
    Specialist: 2,
    Admin: 3,
} as const;
export type UserRole = typeof UserRole[keyof typeof UserRole];

// Project Status Types
export const ProjectStatusType = {
    Hiring: 1,
    InProgress: 2,
    Completed: 3,
    OnHold: 4,
    Cancelled: 5,
} as const;
export type ProjectStatusType = typeof ProjectStatusType[keyof typeof ProjectStatusType];

// Project Request Types
export const ProjectRequestType = {
    Invitation: 1,
    Application: 2,
} as const;
export type ProjectRequestType = typeof ProjectRequestType[keyof typeof ProjectRequestType];

// Search Entity Types
export const SearchEntityType = {
    Projects: 1,
    Users: 2,
    All: 3,
} as const;
export type SearchEntityType = typeof SearchEntityType[keyof typeof SearchEntityType];

// Search Sort By
export const SearchSortBy = {
    Relevance: 1,
    CreatedAt: 2,
    Name: 3,
    Deadline: 4,
} as const;
export type SearchSortBy = typeof SearchSortBy[keyof typeof SearchSortBy];

// Sort Direction
export const SortDirection = {
    Ascending: 1,
    Descending: 2,
} as const;
export type SortDirection = typeof SortDirection[keyof typeof SortDirection];
