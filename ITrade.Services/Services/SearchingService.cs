using ITrade.DB;
using ITrade.DB.Enums;
using ITrade.Services.Interfaces;
using ITrade.Services.Requests;
using ITrade.Services.Responses;
using Microsoft.EntityFrameworkCore;

namespace ITrade.Services.Services
{
    public class SearchingService(
        Context context,
        ICurrentUserService currentUserService
        ) : ISearchingService
    {
        public async Task<SearchResponse> SearchAsync(SearchRequest request)
        {
            var projects = new List<ProjectResponse>();
            var users = new List<UserResponse>();
            var totalProjects = 0;
            var totalUsers = 0;

            var searchProjects = request.EntityType is null 
                or SearchEntityType.All 
                or SearchEntityType.Projects;

            var searchUsers = request.EntityType is null 
                or SearchEntityType.All 
                or SearchEntityType.Users;

            if (searchProjects)
            {
                var (projectResults, projectCount) = await SearchProjectsAsync(request);
                projects = projectResults;
                totalProjects = projectCount;
            }

            if (searchUsers)
            {
                var (userResults, userCount) = await SearchUsersAsync(request);
                users = userResults;
                totalUsers = userCount;
            }

            return new SearchResponse(
                projects,
                users,
                totalProjects,
                totalUsers,
                request.Page,
                request.PageSize
            );
        }

        private async Task<(List<ProjectResponse>, int)> SearchProjectsAsync(SearchRequest request)
        {
            var query = context.Projects
                .Where(p => !p.IsDeleted)
                .AsQueryable();

            // Text search filter
            if (!string.IsNullOrWhiteSpace(request.Query))
            {
                var searchTerm = request.Query.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchTerm)) ||
                    p.ProjectTags.Any(pt => pt.Tag.Name.ToLower().Contains(searchTerm))
                );
            }

            // Tag filter
            if (request.TagIds is { Count: > 0 })
            {
                query = query.Where(p =>
                    p.ProjectTags.Any(pt => request.TagIds.Contains(pt.TagId))
                );
            }

            // Project status filter
            if (request.ProjectStatusId.HasValue)
            {
                query = query.Where(p => p.ProjectStatusTypeId == request.ProjectStatusId.Value);
            }

            // Deadline range filter
            if (request.DeadlineFrom.HasValue)
            {
                query = query.Where(p => p.Deadline >= request.DeadlineFrom.Value);
            }
            if (request.DeadlineTo.HasValue)
            {
                query = query.Where(p => p.Deadline <= request.DeadlineTo.Value);
            }

            // Created date range filter
            if (request.CreatedFrom.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= request.CreatedFrom.Value);
            }
            if (request.CreatedTo.HasValue)
            {
                query = query.Where(p => p.CreatedAt <= request.CreatedTo.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplyProjectSorting(query, request.SortBy, request.SortDirection);

            // Apply pagination
            var skip = (request.Page - 1) * request.PageSize;
            query = query.Skip(skip).Take(request.PageSize);

            var results = await query
                .Select(p => new ProjectResponse(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.OwnerId,
                    p.Owner.Username,
                    p.WorkerId,
                    p.Worker != null ? p.Worker.Username : null,
                    p.Deadline,
                    p.ProjectStatusTypeId,
                    p.ProjectStatusType.Name,
                    p.ProjectTags
                        .Select(pt => new ProjectTagResponse(pt.Id, pt.Tag.Name))
                        .ToList(),
                    p.CreatedAt,
                    p.UpdatedAt
                ))
                .ToListAsync();

            return (results, totalCount);
        }

        private async Task<(List<UserResponse>, int)> SearchUsersAsync(SearchRequest request)
        {
            var query = context.Users
                .Where(u => !u.IsDeleted)
                .Where(u => u.UserRoleId != (int)UserRoleEnum.Admin)
                .AsQueryable();

            // Text search filter
            if (!string.IsNullOrWhiteSpace(request.Query))
            {
                var searchTerm = request.Query.ToLower();
                query = query.Where(u =>
                    u.Username.ToLower().Contains(searchTerm) ||
                    u.UserProfileTags.Any(pt => pt.Tag.Name.ToLower().Contains(searchTerm))
                );
            }

            // Tag filter
            if (request.TagIds is { Count: > 0 })
            {
                query = query.Where(u =>
                    u.UserProfileTags.Any(pt => request.TagIds.Contains(pt.TagId))
                );
            }

            // User role filter
            if (request.UserRoleId.HasValue)
            {
                query = query.Where(u => u.UserRoleId == request.UserRoleId.Value);
            }

            // Created date range filter
            if (request.CreatedFrom.HasValue)
            {
                query = query.Where(u => u.CreatedAt >= request.CreatedFrom.Value);
            }
            if (request.CreatedTo.HasValue)
            {
                query = query.Where(u => u.CreatedAt <= request.CreatedTo.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplyUserSorting(query, request.SortBy, request.SortDirection);

            // Apply pagination
            var skip = (request.Page - 1) * request.PageSize;
            query = query.Skip(skip).Take(request.PageSize);

            var results = await query
                .Select(u => new UserResponse(
                    u.Id,
                    u.Username,
                    u.UserRole.Name
                ))
                .ToListAsync();

            return (results, totalCount);
        }

        public async Task<SearchResponse> SearchDeletedAsync(string? query)
        {
            var searchTerm = query?.ToLower() ?? "";

            var projectQuery = context.Projects
                .IgnoreQueryFilters()
                .Where(p => p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                projectQuery = projectQuery.Where(p => p.Name.ToLower().Contains(searchTerm));
            }

            var totalProjects = await projectQuery.CountAsync();

            var projects = await projectQuery
                .OrderByDescending(p => p.UpdatedAt)
                .Select(p => new ProjectResponse(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.OwnerId,
                    p.Owner.Username,
                    p.WorkerId,
                    p.Worker != null ? p.Worker.Username : null,
                    p.Deadline,
                    p.ProjectStatusTypeId,
                    p.ProjectStatusType.Name,
                    p.ProjectTags
                        .Select(pt => new ProjectTagResponse(pt.Id, pt.Tag.Name))
                        .ToList(),
                    p.CreatedAt,
                    p.UpdatedAt
                ))
                .ToListAsync();

            var userQuery = context.Users
                .IgnoreQueryFilters()
                .Where(u => u.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                userQuery = userQuery.Where(u => u.Username.ToLower().Contains(searchTerm));
            }

            var totalUsers = await userQuery.CountAsync();

            var users = await userQuery
                .OrderByDescending(u => u.UpdatedAt)
                .Select(u => new UserResponse(
                    u.Id,
                    u.Username,
                    u.UserRole.Name
                ))
                .ToListAsync();

            return new SearchResponse(
                projects,
                users,
                totalProjects,
                totalUsers,
                1,
                totalProjects + totalUsers
            );
        }

        private static IQueryable<DB.Entities.Project> ApplyProjectSorting(
            IQueryable<DB.Entities.Project> query,
            SearchSortBy sortBy,
            SortDirection direction)
        {
            var isDescending = direction == SortDirection.Descending;

            return sortBy switch
            {
                SearchSortBy.Name => isDescending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),
                SearchSortBy.CreatedAt => isDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt),
                SearchSortBy.Deadline => isDescending
                    ? query.OrderByDescending(p => p.Deadline)
                    : query.OrderBy(p => p.Deadline),
                _ => isDescending
                    ? query.OrderByDescending(p => p.CreatedAt)
                    : query.OrderBy(p => p.CreatedAt)
            };
        }

        private static IQueryable<DB.Entities.User> ApplyUserSorting(
            IQueryable<DB.Entities.User> query,
            SearchSortBy sortBy,
            SortDirection direction)
        {
            var isDescending = direction == SortDirection.Descending;

            return sortBy switch
            {
                SearchSortBy.Name => isDescending
                    ? query.OrderByDescending(u => u.Username)
                    : query.OrderBy(u => u.Username),
                SearchSortBy.CreatedAt => isDescending
                    ? query.OrderByDescending(u => u.CreatedAt)
                    : query.OrderBy(u => u.CreatedAt),
                SearchSortBy.Relevance => isDescending
                    ? query.OrderByDescending(u => u.ReceivedReviews.Any() 
                        ? u.ReceivedReviews.Average(r => r.Rating) 
                        : 0)
                    : query.OrderBy(u => u.ReceivedReviews.Any() 
                        ? u.ReceivedReviews.Average(r => r.Rating) 
                        : 0),
                _ => isDescending
                    ? query.OrderByDescending(u => u.CreatedAt)
                    : query.OrderBy(u => u.CreatedAt)
            };
        }
    }
}
