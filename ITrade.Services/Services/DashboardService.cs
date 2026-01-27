using ITrade.DB;
using ITrade.DB.Enums;
using ITrade.Services.Interfaces;
using ITrade.Services.Responses;

namespace ITrade.Services.Services
{
    public class DashboardService(
        Context context,
        ICurrentUserService currentUserService
    ) : IDashboardService
    {
        public async Task<DashboardResponse> GetDashboardAsync()
        {
            return currentUserService.UserRole switch
            {
                UserRoleEnum.Client => await BuildClientDashboardAsync(),
                UserRoleEnum.Specialist => await BuildSpecialistDashboardAsync(),
                UserRoleEnum.Admin => await BuildAdminDashboardAsync(),
                _ => throw new InvalidOperationException("Invalid user role.")
            };
        }

        private async Task<DashboardResponse> BuildSpecialistDashboardAsync()
        {
            throw new NotImplementedException();
        }

        private async Task<DashboardResponse> BuildClientDashboardAsync()
        {
            throw new NotImplementedException();
        }

        private async Task<DashboardResponse> BuildAdminDashboardAsync()
        {
            throw new NotImplementedException();
        }
    }
}
