using ITrade.DB;
using ITrade.DB.Enums;
using ITrade.Services.Interfaces;
using ITrade.Services.Responses;
using Microsoft.EntityFrameworkCore;

namespace ITrade.Services.Services
{
    public class DashboardService(
        Context context,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        IMatchingService matchingService
    ) : IDashboardService
    {
        public async Task<DashboardResponse> GetDashboardAsync()
        {
            return currentUserService.UserRole switch
            {
                UserRoleEnum.Client => await BuildClientDashboardAsync(),
                UserRoleEnum.Specialist => await BuildSpecialistDashboardAsync(),
                UserRoleEnum.Admin => await BuildAdminDashboardAsync(),
                _ => throw new InvalidOperationException("Invalid user role.")
            };
        }

        private async Task<DashboardResponse> BuildSpecialistDashboardAsync()
        {
            var userId = currentUserService.UserId;

            var unreadCount = await notificationService.GetUnreadNotificationCountAsync();
            var pendingInvitations = await GetPendingInvitationsAsync(userId);
            var activeProjects = await GetActiveProjectsForSpecialistAsync(userId);
            var recommendedProjects = await matchingService.RecommandProjectsForSpecialist();

            return new DashboardSpecialistResponse(
                UnreadNotificationCount: unreadCount,
                PendingInvitations: pendingInvitations,
                ActiveProjects: activeProjects,
                RecommendedProjects: recommendedProjects
            );
        }

        private async Task<DashboardResponse> BuildClientDashboardAsync()
        {
            var userId = currentUserService.UserId;

            var unreadCount = await notificationService.GetUnreadNotificationCountAsync();
            var pendingApplications = await GetPendingApplicationsAsync(userId);
            var activeProjectCount = await GetActiveProjectCountForClientAsync(userId);
            var openProjects = await GetOpenProjectsWithMatchesAsync(userId);

            return new DashboardClientResponse(
                UnreadNotificationCount: unreadCount,
                PendingApplications: pendingApplications,
                OpenProjectsWithMatches: openProjects,
                ActiveProjectCount: activeProjectCount
            );
        }

        private async Task<DashboardResponse> BuildAdminDashboardAsync()
        {
            var unreadCount = await notificationService.GetUnreadNotificationCountAsync();

            var tags = await context.Tags.ToListAsync();

            return new DashboardAdminResponse(
                UnreadNotificationCount: unreadCount,
                Tags: tags
            );
        }

        private async Task<ICollection<RequestResponse>> GetPendingInvitationsAsync(int userId)
        {
            return await context.Requests
                .Where(r => r.ReceiverId == userId
                    && r.RequestTypeId == (int)ProjectRequestTypeEnum.Invitation
                    && r.Accepted == null)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RequestResponse(
                    r.Id,
                    r.Message,
                    r.SenderId,
                    r.Sender.Username,
                    r.ReceiverId,
                    r.Receiver.Username,
                    r.ProjectId,
                    r.Project.Name,
                    ((ProjectRequestTypeEnum)r.RequestTypeId).ToString(),
                    r.Accepted,
                    r.CreatedAt
                ))
                .ToListAsync();
        }

        private async Task<ICollection<RequestResponse>> GetPendingApplicationsAsync(int userId)
        {
            return await context.Requests
                .Where(r => r.Project.OwnerId == userId
                    && r.RequestTypeId == (int)ProjectRequestTypeEnum.Application
                    && r.Accepted == null)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RequestResponse(
                    r.Id,
                    r.Message,
                    r.SenderId,
                    r.Sender.Username,
                    r.ReceiverId,
                    r.Receiver.Username,
                    r.ProjectId,
                    r.Project.Name,
                    ((ProjectRequestTypeEnum)r.RequestTypeId).ToString(),
                    r.Accepted,
                    r.CreatedAt
                ))
                .ToListAsync();
        }

        private async Task<ICollection<ProjectResponse>> GetActiveProjectsForSpecialistAsync(int userId)
        {
            return await context.Projects
                .Where(p => p.WorkerId == userId
                    && !p.IsDeleted
                    && (p.ProjectStatusTypeId == (int)ProjectStatusTypeEnum.InProgress
                        || p.ProjectStatusTypeId == (int)ProjectStatusTypeEnum.OnHold))
                .OrderBy(p => p.Deadline)
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
        }

        private async Task<int> GetActiveProjectCountForClientAsync(int userId)
        {
            return await context.Projects
                .Where(p => p.OwnerId == userId
                    && !p.IsDeleted
                    && p.ProjectStatusTypeId != (int)ProjectStatusTypeEnum.Completed
                    && p.ProjectStatusTypeId != (int)ProjectStatusTypeEnum.Cancelled)
                .CountAsync();
        }

        private async Task<ICollection<ProjectWithMatchesResponse>> GetOpenProjectsWithMatchesAsync(int userId)
        {
            // Get projects that are in "Hiring" status (no worker assigned yet)
            var openProjects = await context.Projects
                .Where(p => p.OwnerId == userId
                    && !p.IsDeleted
                    && p.WorkerId == null
                    && p.ProjectStatusTypeId == (int)ProjectStatusTypeEnum.Hiring)
                .OrderByDescending(p => p.CreatedAt)
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

            // Get recommended specialists for each open project
            var result = new List<ProjectWithMatchesResponse>();
            foreach (var project in openProjects)
            {
                var recommendedSpecialists = await matchingService.RecommandSpecialistsForProject(project.Id);
                result.Add(new ProjectWithMatchesResponse(project, recommendedSpecialists));
            }

            return result;
        }
    }
}
