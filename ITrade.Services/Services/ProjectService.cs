using ITrade.DB;
using ITrade.DB.Entities;
using ITrade.DB.Enums;
using ITrade.Services.Interfaces;
using ITrade.Services.Requests;
using ITrade.Services.Responses;
using Microsoft.EntityFrameworkCore;

namespace ITrade.Services.Services
{
    public class ProjectService(
        Context context,
        ICurrentUserService currentUserService
    ) : IProjectService
    {
        public async Task<ICollection<ProjectResponse>> GetUserProjectsAsync()
        {
            switch (currentUserService.UserRole)
            {
                case UserRoleEnum.Client:
                    return await GetClientProjectsAsync();
                case UserRoleEnum.Specialist:
                    return await GetSpecialistProjectsAsync();
                case UserRoleEnum.Admin:
                    throw new UnauthorizedAccessException("Invalid user.");
                default:
                    throw new UnauthorizedAccessException("Invalid user.");
            }
        }

        public async Task<ProjectResponse> GetProjectAsync(int projectId)
        {
            return await context.Projects
                .Where(p => p.Id == projectId)
                .Select(p => new ProjectResponse(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.OwnerId,
                    p.Owner.FullName,
                    p.WorkerId,
                    p.Worker != null ? p.Worker.FullName : null,
                    p.Deadline,
                    p.ProjectStatusTypeId,
                    p.ProjectStatusType.Name,
                    p.ProjectTags
                        .Select(pt => new ProjectTagResponse(
                            pt.Id,
                            pt.Tag.Id,
                            pt.Tag.Name,
                            pt.ProjectId
                        ))
                        .ToList(),
                    p.CreatedAt,
                    p.UpdatedAt
                ))
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException("Project not found.");
        }

        public async Task<ICollection<ProjectResponse>> SearchProjectsAsync(string query)
        {
            return await context.Projects
                .Where(p => p.Name.Contains(query))
                .Select(p => new ProjectResponse(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.OwnerId,
                    p.Owner.FullName,
                    p.WorkerId,
                    p.Worker != null ? p.Worker.FullName : null,
                    p.Deadline,
                    p.ProjectStatusTypeId,
                    p.ProjectStatusType.Name,
                    p.ProjectTags
                        .Select(pt => new ProjectTagResponse(
                            pt.Id,
                            pt.Tag.Id,
                            pt.Tag.Name,
                            pt.ProjectId
                        ))
                        .ToList(),
                    p.CreatedAt,
                    p.UpdatedAt
                ))
                .ToListAsync();
        }

        public async Task<int> CreateProjectAsync(ProjectReq projectRequest)
        {
            ValidateProjectReq(projectRequest);
            if (currentUserService.UserRole != UserRoleEnum.Client)
            {
                throw new UnauthorizedAccessException("Only clients can create projects.");
            }

            var newProject = new Project
            {
                Name = projectRequest.Name,
                Description = projectRequest.Description,
                Deadline = projectRequest.Deadline,
                ProjectStatusTypeId = (int)projectRequest.Status,
                OwnerId = currentUserService.UserId
            };

            await context.Projects.AddAsync(newProject);
            await context.SaveChangesAsync();

            return newProject.Id;
        }

        public async Task SoftDeleteProjectAsync(int projectId)
        {
            var project = await context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId)
                ?? throw new KeyNotFoundException("Project not found.");

            if (project.OwnerId != currentUserService.UserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this project.");
            }

            project.IsDeleted = true;
            project.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
        }

        public async Task UpdateProjectAsync(int projectId, ProjectUpdateReq projectRequest)
        {
            ValidateProjectUpdateReq(projectRequest);

            var project = await context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId)
                ?? throw new KeyNotFoundException("Project not found.");

            if (project.OwnerId != currentUserService.UserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this project.");
            }

            if (projectRequest.Name != null)
            {
                project.Name = projectRequest.Name;
            }
            if (projectRequest.Description != null)
            {
                project.Description = projectRequest.Description;
            }
            if (projectRequest.Deadline != null)
            {
                project.Deadline = (DateTime)projectRequest.Deadline;
            }
            if (projectRequest.Status != null)
            {
                project.ProjectStatusTypeId = (int)projectRequest.Status;
            }

            project.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }

        private async Task<ICollection<ProjectResponse>> GetClientProjectsAsync()
        {
            var userId = currentUserService.UserId;

            return await context.Projects
                .Where(p => p.OwnerId == userId)
                .Select(p => new ProjectResponse(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.OwnerId,
                    p.Owner.FullName,
                    p.WorkerId,
                    p.Worker != null ? p.Worker.FullName : null,
                    p.Deadline,
                    p.ProjectStatusTypeId,
                    p.ProjectStatusType.Name,
                    p.ProjectTags
                        .Select(pt => new ProjectTagResponse(
                            pt.Id,
                            pt.Tag.Id,
                            pt.Tag.Name,
                            pt.ProjectId
                        ))
                        .ToList(),
                    p.CreatedAt,
                    p.UpdatedAt
                ))
                .ToListAsync();
        }

        private async Task<ICollection<ProjectResponse>> GetSpecialistProjectsAsync()
        {
            var userId = currentUserService.UserId;

            return await context.Projects
                .Where(p => p.WorkerId == userId)
                .Select(p => new ProjectResponse(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.OwnerId,
                    p.Owner.FullName,
                    p.WorkerId,
                    p.Worker.FullName,
                    p.Deadline,
                    p.ProjectStatusTypeId,
                    p.ProjectStatusType.Name,
                    p.ProjectTags
                        .Select(pt => new ProjectTagResponse(
                            pt.Id,
                            pt.Tag.Id,
                            pt.Tag.Name,
                            pt.ProjectId
                        ))
                        .ToList(),
                    p.CreatedAt,
                    p.UpdatedAt
                ))
                .ToListAsync();
        }

        private void ValidateProjectReq(ProjectReq projectRequest)
        {
            if (!Enum.IsDefined(typeof(ProjectStatusTypeEnum), projectRequest.Status))
            {
                throw new ArgumentException("Invalid project status type.", nameof(projectRequest));
            }
            if (string.IsNullOrEmpty(projectRequest.Name))
            {
                throw new ArgumentException("Project name cannot be null or empty.", nameof(projectRequest));
            }
            if (projectRequest.Deadline < DateTime.UtcNow)
            {
                throw new ArgumentException("Project deadline cannot be in the past.", nameof(projectRequest));
            }
            if (projectRequest.Name.Length > 200)
            {
                throw new ArgumentException("Project name cannot exceed 200 characters.", nameof(projectRequest));
            }
            if (projectRequest.Description != null && projectRequest.Description.Length > 1000)
            {
                throw new ArgumentException("Project description cannot exceed 1000 characters.", nameof(projectRequest));
            }
        }

        private void ValidateProjectUpdateReq(ProjectUpdateReq projectRequest)
        {
            if (projectRequest.Status != null && !Enum.IsDefined(typeof(ProjectStatusTypeEnum), projectRequest.Status))
            {
                throw new ArgumentException("Invalid project status type.", nameof(projectRequest));
            }
            if (projectRequest.Name != null && projectRequest.Name.Length > 200)
            {
                throw new ArgumentException("Project name cannot exceed 200 characters.", nameof(projectRequest));
            }
            if (projectRequest.Description != null && projectRequest.Description.Length > 1000)
            {
                throw new ArgumentException("Project description cannot exceed 1000 characters.", nameof(projectRequest));
            }
            if (projectRequest.Deadline != null && projectRequest.Deadline < DateTime.UtcNow)
            {
                throw new ArgumentException("Project deadline cannot be in the past.", nameof(projectRequest));
            }
        }
    }
}
