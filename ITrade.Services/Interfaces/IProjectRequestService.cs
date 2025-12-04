using ITrade.Services.Requests;
using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IProjectRequestService
    {
        Task<UserRequestsResponse> GetUserRequestsAsync();
        Task<int> CreateProjectRequestAsync(ProjectRequestReq projectRequest);
        Task ResolveRequestAsync(int projectRequestId, bool accepted);
        Task DeleteProjectRequestAsync(int projectRequestId);
    }
}
