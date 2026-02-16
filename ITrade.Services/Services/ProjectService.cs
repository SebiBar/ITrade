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
        public async Task<ProjectResponse> GetProjectAsync(int projectId)
        {
            return await context.Projects
                .Where(p => p.Id == projectId)
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
                            .Select(pt => new ProjectTagResponse(
                                pt.Id,
                                pt.Tag.Name
                            ))
                            .ToList(),
                        p.CreatedAt,
                        p.UpdatedAt
                    ))
                    .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException("Project not found.");
        }

        public async Task<int> CreateProjectAsync(ProjectRequest projectRequest)
        {
            ValidateProjectReq(projectRequest);
            if (currentUserService.UserRole != UserRoleEnum.Client)
            {
                throw new InvalidOperationException("Only clients can create projects.");
            }

            var newProject = new Project
            {
                Name = projectRequest.Name,
                Description = projectRequest.Description,
                Deadline = projectRequest.Deadline,
                OwnerId = currentUserService.UserId
            };

            if (projectRequest.Status != null)
            {
                newProject.ProjectStatusTypeId = (int)projectRequest.Status;
            }

            await context.Projects.AddAsync(newProject);

            foreach (var tagId in projectRequest.TagIds)
            {
                if (!await context.Tags.AnyAsync(t => t.Id == tagId))
                {
                    throw new KeyNotFoundException($"Tag with ID {tagId} not found.");
                }

                var projectTag = new ProjectTag
                {
                    ProjectId = newProject.Id,
                    TagId = tagId
                };
                newProject.ProjectTags.Add(projectTag);
            }

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
                throw new InvalidOperationException("You do not have permission to delete this project.");
            }

            project.IsDeleted = true;
            project.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
        }

        public async Task<List<ProjectResponse>> GetDeletedProjectsAsync()
        {
            var query = context.Projects
                .IgnoreQueryFilters()
                .Where(p => p.IsDeleted);

            // Admins can see all deleted projects, others only their own
            if (currentUserService.UserRole != UserRoleEnum.Admin)
            {
                query = query.Where(p => p.OwnerId == currentUserService.UserId);
            }

            return await query
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

        public async Task RestoreProjectAsync(int projectId)
        {
            var project = await context.Projects
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == projectId && p.IsDeleted)
                ?? throw new KeyNotFoundException("Deleted project not found.");

            // Admins can restore any project, others only their own
            if (currentUserService.UserRole != UserRoleEnum.Admin && project.OwnerId != currentUserService.UserId)
            {
                throw new InvalidOperationException("You do not have permission to restore this project.");
            }

            project.IsDeleted = false;
            project.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
        }

        public async Task HardDeleteProjectAsync(int projectId)
        {
            var project = await context.Projects
                .IgnoreQueryFilters()
                .Include(p => p.ProjectTags)
                .Include(p => p.Requests)
                .FirstOrDefaultAsync(p => p.Id == projectId)
                ?? throw new KeyNotFoundException("Project not found.");

            context.ProjectTags.RemoveRange(project.ProjectTags);
            context.Requests.RemoveRange(project.Requests);
            context.Projects.Remove(project);

            await context.SaveChangesAsync();
        }

        public async Task UpdateProjectAsync(int projectId, ProjectUpdateRequest projectRequest)
        {
            ValidateProjectUpdateReq(projectRequest);

            var project = await context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId)
                ?? throw new KeyNotFoundException("Project not found.");

            if (project.OwnerId != currentUserService.UserId)
            {
                throw new InvalidOperationException("You do not have permission to update this project.");
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

        private void ValidateProjectReq(ProjectRequest projectRequest)
        {
            if (projectRequest.Status != null && !Enum.IsDefined(typeof(ProjectStatusTypeEnum), projectRequest.Status))
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

        private void ValidateProjectUpdateReq(ProjectUpdateRequest projectRequest)
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
