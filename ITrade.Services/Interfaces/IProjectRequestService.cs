using ITrade.Services.Requests;
using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IProjectRequestService
    {
        Task<ProjectRequestResponse> GetUserRequestsAsync(int senderId, int projectId);
        Task<int> CreateProjectRequestAsync(int senderId, ProjectRequestReq projectRequest);
        Task ResolveRequestAsync(int receiverId, int projectRequestId, bool accepted);
        Task DeleteProjectRequestAsync(int senderId, int projectRequestId);
    }
}
