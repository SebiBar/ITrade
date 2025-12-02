using ITrade.Services.Requests;
using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IProjectService
    {
        Task<ICollection<ProjectResponse>> GetUserProjects();
        Task<ProjectResponse> GetProjectAsync(int projectId);
        Task<ICollection<ProjectResponse>> SearchProjectsAsync(string query);
        Task<int> CreateProjectAsync(ProjectReq projectRequest);
        Task UpdateProjectAsync(int projectId, ProjectReq projectRequest);
        Task SoftDeleteProjectAsync(int projectId);
    }
}
