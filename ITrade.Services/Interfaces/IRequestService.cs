using ITrade.Services.Requests;
using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IRequestService
    {
        Task<UserRequestsResponse> GetUserRequestsAsync();
        Task<bool> UserAlreadyAppliedAsync(int projectId);
        Task<int> CreateRequestAsync(RequestReq projectRequest);
        Task ResolveRequestAsync(int requestId, bool accepted);
        Task DeleteRequestAsync(int requestId);
    }
}
