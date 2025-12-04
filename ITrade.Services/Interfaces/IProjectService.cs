using ITrade.Services.Requests;
using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IProjectService
    {
        Task<ICollection<ProjectResponse>> GetUserProjectsAsync();
        Task<ProjectResponse> GetProjectAsync(int projectId);
        Task<ICollection<ProjectResponse>> SearchProjectsAsync(string query);
        Task<int> CreateProjectAsync(ProjectReq projectRequest);
        Task UpdateProjectAsync(int projectId, ProjectUpdateReq projectRequest);
        Task SoftDeleteProjectAsync(int projectId);
        Task<int> AddProjectTagAsync(int projectId, ProjectTagAddRequest tagId);
        Task DeleteProjectTagAsync(int projectId, int tagId);
    }
}
