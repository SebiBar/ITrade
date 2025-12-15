using ITrade.Services.Requests;
using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IRequestService
    {
        Task<UserRequestsResponse> GetUserRequestsAsync();
        Task<int> CreateRequestAsync(RequestReq projectRequest);
        Task ResolveRequestAsync(int requestId, bool accepted);
        Task DeleteRequestAsync(int requestId);
    }
}
