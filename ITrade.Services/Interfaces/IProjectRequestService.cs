using ITrade.Services.Requests;
using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IProjectRequestService
    {
        Task<int> CreateProjectRequestAsync(int senderId, ProjectRequestRequest projectRequest);
        Task DeleteProjectRequestAsync(int senderId, int projectRequestId);
        Task<ProjectRequestResponse> GetUserRequestsAsync(int senderId, int projectId);
        Task ResolveRequestAsync(int receiverId, int projectRequestId, bool accepted);
    }
}
