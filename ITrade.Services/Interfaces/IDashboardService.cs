using ITrade.Services.Responses;

namespace ITrade.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardResponse> GetDashboardAsync();
    }
}
