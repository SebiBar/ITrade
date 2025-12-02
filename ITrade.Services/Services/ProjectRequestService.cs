using ITrade.Services.Interfaces;
using ITrade.Services.Requests;
using ITrade.Services.Responses;

namespace ITrade.Services.Services
{
    public class ProjectRequestService : IProjectRequestService
    {
        public Task<int> CreateProjectRequestAsync(int senderId, ProjectRequestReq projectRequest)
        {
            throw new NotImplementedException();
        }

        public Task DeleteProjectRequestAsync(int senderId, int projectRequestId)
        {
            throw new NotImplementedException();
        }

        public Task<ProjectRequestResponse> GetUserRequestsAsync(int senderId, int projectId)
        {
            throw new NotImplementedException();
        }

        public Task ResolveRequestAsync(int receiverId, int projectRequestId, bool accepted)
        {
            throw new NotImplementedException();
        }
    }
}
