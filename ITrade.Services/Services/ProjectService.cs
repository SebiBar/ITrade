using ITrade.DB.Entities;
using ITrade.Services.Interfaces;
using ITrade.Services.Requests;
using ITrade.Services.Responses;

namespace ITrade.Services.Services
{
    public class ProjectService : IProjectService
    {
        public Task<int> CreateProjectAsync(ProjectRequestRequest projectRequest)
        {
            throw new NotImplementedException();
        }

        public Task<ProjectResponse> GetProjectAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public Task<ProjectResponse> GetUserProjects()
        {
            throw new NotImplementedException();
        }

        public Task SoftDeleteProjectAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateProjectAsync(int projectId, ProjectRequestRequest projectRequest)
        {
            throw new NotImplementedException();
        }
    }
}
