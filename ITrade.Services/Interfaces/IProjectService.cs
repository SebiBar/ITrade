using ITrade.DB.Entities;
using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IProjectService
    {
        Task<int> CreateProjectAsync(ProjectRequest projectRequest);
        Task UpdateProjectAsync(int projectId, ProjectRequest projectRequest);
        Task SoftDeleteProjectAsync(int projectId);
        Task<ProjectResponse> GetUserProjects();
    }
}
