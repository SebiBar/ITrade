using ITrade.Services.Requests;
using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectResponse> GetProjectAsync(int projectId);
        Task<int> CreateProjectAsync(ProjectRequest projectRequest);
        Task UpdateProjectAsync(int projectId, ProjectUpdateRequest projectRequest);
        Task SoftDeleteProjectAsync(int projectId);
        Task<List<ProjectResponse>> GetDeletedProjectsAsync();
        Task RestoreProjectAsync(int projectId);
        Task HardDeleteProjectAsync(int projectId);
    }
}
