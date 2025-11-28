using ITrade.Services.Requests;
using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IProjectService
    {
        Task<int> CreateProjectAsync(ProjectRequestRequest projectRequest);
        Task UpdateProjectAsync(int projectId, ProjectRequestRequest projectRequest);
        Task SoftDeleteProjectAsync(int projectId);
        Task<ProjectResponse> GetUserProjects();
        Task<ProjectResponse> GetProjectAsync(int projectId);
    }
}
