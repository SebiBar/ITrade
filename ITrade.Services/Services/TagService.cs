using ITrade.DB;
using ITrade.DB.Entities;
using ITrade.DB.Enums;
using ITrade.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITrade.Services.Services
{
    public class TagService(
        Context context,
        ICurrentUserService currentUserService
        ) : ITagService
    {
        public async Task<int> CreateTagAsync(string tagName)
        {
            if (currentUserService.UserRole != UserRoleEnum.Admin)
            {
                throw new ArgumentException("Cannot create tags.");
            }

            if (string.IsNullOrWhiteSpace(tagName) || tagName.Length > 50)
            {
                throw new ArgumentException("Invalid name.", nameof(tagName));
            }

            var newTag = new Tag
            {
                Name = tagName
            };

            context.Tags.Add(newTag);
            await context.SaveChangesAsync();

            return newTag.Id;
        }

        public async Task DeleteTagAsync(int tagId)
        {
            if (currentUserService.UserRole != UserRoleEnum.Admin)
            {
                throw new ArgumentException("Cannot delete tags.");
            }

            var tag = await context.Tags.FirstOrDefaultAsync(t => t.Id == tagId)
                ?? throw new ArgumentException("Invalid tagId");

            context.Tags.Remove(tag);
            await context.SaveChangesAsync();
        }

        public async Task<ICollection<Tag>> SearchTagsAsync(string tagName)
        {
            return await context.Tags.Where(t => t.Name.Contains(tagName)).ToListAsync();
        }

        public async Task<int> AddProfileTagAsync(int tagId)
        {
            if (currentUserService.UserRole != UserRoleEnum.Specialist)
            {
                throw new ArgumentException("Only specialists can add profile tags.");
            }

            var tagName = await context.Tags
                .Where(t => t.Id == tagId)
                .Select(t => t.Name)
                .FirstOrDefaultAsync()
                ?? throw new ArgumentException("Invalid tagId", nameof(tagId));

            var newTag = new UserProfileTag
            {
                TagId = tagId,
                UserId = currentUserService.UserId,
                TagName = tagName
            };

            await context.UserProfileTags.AddAsync(newTag);
            await context.SaveChangesAsync();

            return newTag.Id;
        }

        public async Task RemoveProfileTagAsync(int tagId)
        {
            var profileTag = await context.UserProfileTags
                .Where(upt => upt.TagId == tagId && upt.UserId == currentUserService.UserId)
                .FirstOrDefaultAsync()
                ?? throw new Exception("Profile tag not found.");

            context.UserProfileTags.Remove(profileTag);
            await context.SaveChangesAsync();
        }

        public async Task<int> AddProjectTagAsync(int projectId, int tagId)
        {
            if (currentUserService.UserRole != UserRoleEnum.Client)
            {
                throw new ArgumentException("Only clients can add project tags.");
            }

            var project = await context.Projects
                .Include(p => p.ProjectTags)
                .FirstOrDefaultAsync(p => p.Id == projectId)
                ?? throw new KeyNotFoundException("Project not found.");

            if (project.OwnerId != currentUserService.UserId)
            {
                throw new InvalidOperationException("You do not have permission to modify this project.");
            }

            if (project.ProjectTags.Any(pt => pt.TagId == tagId))
            {
                throw new ArgumentException("Tag already exists for this project.", nameof(tagId));
            }

            var projectTag = new ProjectTag
            {
                ProjectId = projectId,
                TagId = tagId
            };

            project.UpdatedAt = DateTime.UtcNow;
            await context.ProjectTags.AddAsync(projectTag);

            await context.SaveChangesAsync();

            return projectTag.Id;
        }

        public async Task RemoveProjectTagAsync(int projectId, int tagId)
        {
            var project = await context.Projects
                .Include(p => p.ProjectTags)
                .FirstOrDefaultAsync(p => p.Id == projectId)
                ?? throw new KeyNotFoundException("Project not found.");

            if (project.OwnerId != currentUserService.UserId)
            {
                throw new InvalidOperationException("You do not have permission to modify this project.");
            }

            var projectTag = project.ProjectTags
                .FirstOrDefault(pt => pt.TagId == tagId)
                ?? throw new KeyNotFoundException("Tag not found for this project.");

            project.UpdatedAt = DateTime.UtcNow;
            context.ProjectTags.Remove(projectTag);

            await context.SaveChangesAsync();
        }

        public async Task<ICollection<Tag>> GetAllTagsAsync()
        {
            return await context.Tags.ToListAsync();
        }
    }
}
