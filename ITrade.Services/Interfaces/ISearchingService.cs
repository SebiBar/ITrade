using ITrade.Services.Requests;
using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface ISearchingService
    {
        Task<SearchResponse> SearchAsync(SearchRequest request);
        Task<SearchResponse> SearchDeletedAsync(string? query);
    }
}
