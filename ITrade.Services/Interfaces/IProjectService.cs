using ITrade.Services.Requests;
using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IProjectService
    {
        Task<ICollection<ProjectResponse>> GetUserProjectsAsync();
        Task<ICollection<ProjectSummarizedResponse>> GetUserHiringProjectsAsync(int? toBeInvitedId);
        Task<ProjectResponse> GetProjectAsync(int projectId);
        Task<int> CreateProjectAsync(ProjectRequest projectRequest);
        Task UpdateProjectAsync(int projectId, ProjectUpdateRequest projectRequest);
        Task UnassignProjectWorker(int projectId);
        Task SoftDeleteProjectAsync(int projectId);
        Task<ICollection<ProjectResponse>> GetDeletedProjectsAsync();
        Task RestoreProjectAsync(int projectId);
        Task HardDeleteProjectAsync(int projectId);
    }
}
