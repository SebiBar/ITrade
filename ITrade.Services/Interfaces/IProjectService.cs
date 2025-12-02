using ITrade.Services.Requests;
using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IProjectService
    {
        Task<ICollection<ProjectResponse>> GetUserProjects();
        Task<ProjectResponse> GetProjectAsync(int projectId);
        Task<ICollection<ProjectResponse>> SearchProjectsAsync(string query);
        Task<int> CreateProjectAsync(ProjectRequestRequest projectRequest);
        Task UpdateProjectAsync(int projectId, ProjectRequestRequest projectRequest);
        Task SoftDeleteProjectAsync(int projectId);
    }
}
